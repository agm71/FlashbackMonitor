using HtmlAgilityPack;
using System.Linq;
using System.Web;
using FlashbackMonitor.Services;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using DynamicData;

namespace FlashbackMonitor.Utils
{
    public class FlashbackParser
    {
        private static TopicPage topicsPage = new();
        private static readonly string[] htmlTags = ["a", "b", "i", "ul", "li", "img", "ol"];
        private static readonly string flashbackUrl = "https://www.flashback.org";

        public static TopicPage ParseTopicsPage(HtmlDocument doc)
        {
            var TopicTitle = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//meta[contains(@property, 'og:title')]")?.GetAttributeValue("content", null));

            topicsPage.TopicName = HttpUtility.HtmlDecode(TopicTitle);
            List<string> itms = [];

            var postsNode = doc.DocumentNode.SelectSingleNode("//div[contains(@id, 'posts')]");
            var postNodes = postsNode.SelectNodes("./div[contains(@class, 'post')]");
            var pagerNode = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'input-page-jump-xs')]");

            topicsPage.PostItems.Clear();
            topicsPage.PageNumbers.Clear();

            if (pagerNode != null)
            {
                topicsPage.CurrentPage = pagerNode.GetAttributeValue("data-page", 1);

                var pageCount = Convert.ToInt32(pagerNode.GetAttributeValue("data-total-pages", 1));

                for (int i = 1; i <= pageCount; i++)
                {
                    topicsPage.PageNumbers.Add(i.ToString());
                }
            }
            else
            {
                topicsPage.PageNumbers.Add("1");
                topicsPage.CurrentPage = 1;
            }

            foreach (HtmlNode postN in postNodes)
            {
                var postRow = postN.SelectSingleNode(".//div[contains(@class, 'post-row')]");
                var postDate = FlashbackRegexes.PostDateRegex().Match(postN.SelectSingleNode(".//div[contains(@class, 'post-heading')]")?.InnerText?.Trim())?.Value;

                FlashbackPostItem post = new()
                {
                    Author = postRow.SelectSingleNode(".//a[contains(@class, 'post-user-username')]")?.InnerText?.Trim() ??
                    postRow.SelectSingleNode(".//div[contains(@class, 'dropdown')]//strong")?.InnerText?.Trim(),
                    UserRegistration = postRow.SelectSingleNode(".//div[contains(@class, 'post-user-info')]//div")?.InnerText?.Trim(),
                    UserPosts = HttpUtility.HtmlDecode(postRow.SelectSingleNode(".//div[contains(@class, 'post-user-info')]//div[2]")?.InnerText?.Trim()),
                    PostDate = HttpUtility.HtmlDecode(postDate),
                    UserAvatar = postN.SelectSingleNode(".//a[contains(@class, 'post-user-avatar')]//img")?.Attributes["src"]?.Value?.Trim(),
                    UserType = postRow.SelectSingleNode(".//div[contains(@class, 'post-user-title')]")?.InnerText?.Trim(),
                    OnlineStatus = postRow.SelectSingleNode(".//div[contains(@class, 'post-user-title')]//i[contains(@class, 'fa fa-circle')]")?.GetAttributeValue("title", "Okänd status") ?? "Okänd status",
                };

                post.OnlineStatusColor = GetColorFromOnlineStatus(post.OnlineStatus);

                var textContainer = new TextContainer();

                int quoteCounter = 0;
                int spoilerCounter = 0;
                int codeCounter = 0;

                foreach (var node in postRow.SelectSingleNode(".//div[contains(@class, 'post_message')]")?.ChildNodes)
                {
                    if (node.NodeType == HtmlNodeType.Element)
                    {
                        if (node.Name == "br")
                        {
                            post.TextContainers.Add(textContainer);

                            textContainer = new TextContainer();
                            textContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            if (quoteCounter > 0 || spoilerCounter > 0 || codeCounter > 0)
                            {
                                post.TextContainers.Add(textContainer);

                                textContainer = new TextContainer();

                                quoteCounter = 0;
                                spoilerCounter = 0;
                                codeCounter = 0;
                            }

                            continue;
                        }

                        if (htmlTags.Contains(node.Name))
                        {
                            if (node.Name == "a")
                            {
                                var url = node.Attributes["href"].Value;

                                if (url.StartsWith("/"))
                                {
                                    url = $"{flashbackUrl}{url}";
                                }

                                textContainer.TextItems.Add(new TextItem
                                {
                                    Text = HttpUtility.HtmlDecode(node.InnerText.Trim()) + " ",
                                    Kind = TextKind.Link,
                                    AdditionalData = url
                                });

                                continue;
                            }
                            else if (node.Name == "b")
                            {
                                node.InnerText.SplitIntoTextItems(textContainer, TextKind.Bold);

                                continue;
                            }
                            else if (node.Name == "i")
                            {
                                node.InnerText.SplitIntoTextItems(textContainer, TextKind.Italic);

                                continue;
                            }
                            else if (node.Name == "img")
                            {
                                ParseSmiley(node, textContainer);

                                continue;
                            }
                            else if (node.Name == "ol")
                            {
                                if (node.GetAttributeValue("style", null) == "list-style-type: decimal")
                                {
                                    post.TextContainers.Add(textContainer);

                                    List<ITextContainer> liTextContainers = [];

                                    ParseOrderedList(liTextContainers, node);

                                    liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                                    post.TextContainers.AddRange(liTextContainers);

                                    textContainer = new TextContainer();
                                }
                            }
                        }
                        
                        else if (node.HasClass("post-bbcode-quote-wrapper"))
                        {
                            quoteCounter++;
                            var textitem = new TextItem();
                            ParseQuoteWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);
                            continue;
                        }
                        else if (node.HasClass("post-bbcode-code-wrapper"))
                        {
                            codeCounter++;
                            var textitem = new TextItem();
                            ParseCodeWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);
                            continue;
                        }

                        if (node.HasClass("post-bbcode-spoiler-wrapper"))
                        {
                            spoilerCounter++;
                            var textitem = new TextItem();
                            ParseSpoilerWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);
                            continue;
                        }


                        if (node.Name == "ul")
                        {
                            post.TextContainers.Add(textContainer);

                            List<ITextContainer> liTextContainers = [];

                            ParseUnorderedList(liTextContainers, node);

                            liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                            post.TextContainers.AddRange(liTextContainers);

                            textContainer = new TextContainer();
                        }
                    }
                    else if (node.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(node.ParentNode.Name) && !string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        quoteCounter = 0;
                        spoilerCounter = 0;
                        codeCounter = 0;

                        node.InnerText.SplitIntoTextItems(textContainer, TextKind.Text);
                    }
                }

                post.TextContainers.Add(textContainer);
                topicsPage.PostItems.Add(post);
            }

            return topicsPage;
        }

        private static void ParseSmiley(HtmlNode node, ITextContainer textContainer)
        {
            switch (node.Attributes["title"].Value)
            {
                case "Smile":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F642;"),
                        });
                    }
                    break;
                case "Grin":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F601;"),
                        });
                    }
                    break;
                case "Whink":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#128521;"),
                        });
                    }
                    break;
                case "Cry":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#128546;"),
                        });
                    }
                    break;
                case "Tongue":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F61B;"),
                        });
                    }
                    break;
                case "Sad":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F61F;"),
                        });
                    }
                    break;
                case "Sad44":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F62D;"),
                        });
                    }
                    break;
                case "Ohmy":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F62E;"),
                        });
                    }
                    break;
                case "Noexpression":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F610;"),
                        });
                    }
                    break;
                case "Laugh":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F604;"),
                        });
                    }
                    break;
                case "Rolleyes":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F644;"),
                        });
                    }
                    break;
                case "Innocent":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F607;"),
                        });
                    }
                    break;
                case "Whistle":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F617;"),
                        });
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F3B5;"),
                        });
                    }
                    break;
                case "EEK!":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F603;"),
                        });
                    }
                    break;
                case "Cool":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F60E;"),
                        });
                    }
                    break;
                case "Unsure":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F914;"),
                        });
                    }
                    break;
                case "Confused":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F615;"),
                        });
                    }
                    break;
                case "Boxing":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F94A;"),
                        });
                    }
                    break;
                case "Devil":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F608;"),
                        });
                    }
                    break;
                case "Beer":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F37B;"),
                        });
                    }
                    break;
                case "Rant":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F92C;"),
                        });
                    }
                    break;
                case "Thumbsup":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F44D;"),
                        });
                    }
                    break;
                case "Thumbsdown":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F44E;"),
                        });
                    }
                    break;
                case "Angry":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F620;"),
                        });
                    }
                    break;
                case "Yes":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F642;&#x200D;&#x2195;&#xFE0F;"),
                        });
                    }
                    break;
                case "Sick19":
                    {
                        textContainer.TextItems.Add(new TextItem
                        {
                            Kind = TextKind.Smiley,
                            Text = HttpUtility.HtmlDecode("&#x1F92E;"),
                        });
                    }
                    break;
                default:
                    break;
            }
        }

        private static void ParseCodeWrapper(ITextItem textitem, HtmlNode node)
        {
            try
            {
                var codeContainerCollection = new CodeContainerCollection();
                var preElement = node.SelectSingleNode(".//pre[contains(@class, 'post-bbcode-code')]");

                if (preElement != null)
                {
                    var codeLines = preElement?.InnerText?.Split(Environment.NewLine);

                    foreach (var line in codeLines)
                    {
                        var container = new CodeTextContainer();
                        container.TextItems.Add(new TextItem
                        {
                            Text = HttpUtility.HtmlDecode(line),
                            Kind = TextKind.Code
                        });
                        codeContainerCollection.CodeContainers.Add(container);
                    }

                    textitem.CodeContainerCollections.Add(codeContainerCollection);
                }
                else
                {
                    var phpCode = node.SelectSingleNode(".//div[contains(@class, 'alt2 post-bbcode-php')]");

                    if (phpCode != null)
                    {
                        var codeLines = phpCode?.InnerText?.Split(Environment.NewLine);

                        foreach (var line in codeLines)
                        {
                            var container = new CodeTextContainer();
                            container.TextItems.Add(new TextItem
                            {
                                Text = HttpUtility.HtmlDecode(line),
                                Kind = TextKind.Code
                            });
                            codeContainerCollection.CodeContainers.Add(container);
                        }

                        textitem.CodeContainerCollections.Add(codeContainerCollection);
                    }
                }
            }
            catch
            {
                textitem.CodeContainerCollections.Add(new CodeContainerCollection
                {
                    CodeContainers =
                    [
                        new CodeTextContainer
                        {
                            TextItems = [
                                new TextItem {
                                    Text = "[Innehållet stöds inte av Flashback Monitor]",
                                    Kind = TextKind.Bold
                                }
                            ]
                        }
                    ]
                });
            }
        }

        private static string GetColorFromOnlineStatus(string onlineStatus)
        {
            return onlineStatus switch
            {
                "online" => "#009000",
                "offline" => "#900000",
                _ => "transparent",
            };
        }

        private static void ParseOrderedList(List<ITextContainer> textContainers, HtmlNode node)
        {
            var liElements = node.SelectNodes(".//li");

            for (int i = 0; i < liElements.Count; i++)
            {
                var textContainer = new TextContainer();

                var textItem = new TextItem
                {
                    Text = $"{i + 1}. ",
                    Kind = TextKind.Bullet
                };

                textContainer.TextItems.Add(textItem);

                foreach (var child in liElements[i].ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element)
                    {
                        if (child.Name == "a")
                        {
                            var url = child.Attributes["href"].Value;

                            if (url.StartsWith("/"))
                            {
                                url = $"{flashbackUrl}{url}";
                            }

                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Link, url);
                        }
                        else if (child.Name == "b")
                        {
                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Bold);
                        }
                        else if (child.Name == "i")
                        {
                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Italic);
                        }
                    }
                    else if (child.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(child.Name) && !string.IsNullOrWhiteSpace(child.InnerText))
                    {
                        child.InnerText.SplitIntoTextItems(textContainer, TextKind.Text);
                    }
                }

                textContainers.Add(textContainer);
            }
        }

        private static void ParseUnorderedList(List<ITextContainer> textContainers, HtmlNode node)
        {
            var liElements = node.SelectNodes(".//li");

            foreach (var liElement in liElements)
            {
                var textContainer = new TextContainer();

                var textItem = new TextItem
                {
                    Text = HttpUtility.HtmlDecode("&#x2022; "),
                    Kind = TextKind.Bullet
                };

                textContainer.TextItems.Add(textItem);

                foreach (var child in liElement.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element)
                    {
                        if (child.Name == "a")
                        {
                            var url = child.Attributes["href"].Value;

                            if (url.StartsWith("/"))
                            {
                                url = $"{flashbackUrl}{url}";
                            }

                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Link, url);
                        }
                        else if (child.Name == "b")
                        {
                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Bold);
                        }
                        else if (child.Name == "i")
                        {
                            child.InnerText.SplitIntoTextItems(textContainer, TextKind.Italic);
                        }
                    }
                    else if (child.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(child.Name) && !string.IsNullOrWhiteSpace(child.InnerText))
                    {
                        child.InnerText.SplitIntoTextItems(textContainer, TextKind.Text);
                    }
                }

                textContainers.Add(textContainer);
            }
        }

        private static void ParseSpoilerWrapper(ITextItem item, HtmlNode node)
        {
            SpoilerTextContainer spoilerContainer = new();
            List<string> itms = [];
            int codeCounter = 0;
            int quoteCounter = 0;
            SpoilerContainerCollection spoilerContainerCollection = new();

            var spoilerContentNodes = node.SelectNodes(".//div[contains(@class, 'alt2 post-bbcode-spoiler hidden')]");

            if (spoilerContentNodes != null)
            {
                foreach (var snode in spoilerContentNodes.SelectMany(x => x.ChildNodes))
                {
                    if (snode.NodeType is HtmlNodeType.Element)
                    {
                        if (snode.Name == "br")
                        {
                            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
                            spoilerContainer = new();
                            spoilerContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            if (codeCounter > 0 || quoteCounter > 0)
                            {
                                spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
                                spoilerContainer = new();
                                spoilerContainer.TextItems.Add(new TextItem
                                {
                                    Kind = TextKind.LineBreak
                                });

                                codeCounter = 0;
                                quoteCounter = 0;
                            }

                            spoilerContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            continue;
                        }

                        if (htmlTags.Contains(snode.Name))
                        {
                            if (snode.Name == "a")
                            {
                                if (snode.Name == "a")
                                {
                                    var url = snode.Attributes["href"].Value;

                                    if (url.StartsWith("/"))
                                    {
                                        url = $"{flashbackUrl}{url}";
                                    }

                                    spoilerContainer.TextItems.Add(new SpoilerTextItem
                                    {
                                        Text = HttpUtility.HtmlDecode(snode.InnerText.Trim()) + " ",
                                        Kind = TextKind.Link,
                                        AdditionalData = url
                                    });
                                }
                            }
                            else if (snode.Name == "b")
                            {
                                snode.InnerText.SplitIntoTextItems(spoilerContainer, TextKind.Bold);
                            }
                            else if (snode.Name == "i")
                            {
                                snode.InnerText.SplitIntoTextItems(spoilerContainer, TextKind.Italic);
                            }
                            else if (snode.Name == "ul")
                            {
                                spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseUnorderedList(liTextContainers, snode);

                                liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                                spoilerContainerCollection.SpoilerContainers.AddRange(liTextContainers);

                                spoilerContainer = new SpoilerTextContainer();
                            }
                            else if (snode.Name == "ol")
                            {
                                spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseOrderedList(liTextContainers, snode);

                                liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                                spoilerContainerCollection.SpoilerContainers.AddRange(liTextContainers);

                                spoilerContainer = new SpoilerTextContainer();
                            }
                            else if (snode.Name == "img")
                            {
                                ParseSmiley(snode, spoilerContainer);

                                continue;
                            }
                        }
                        else if (snode.HasClass("post-bbcode-code-wrapper"))
                        {
                            codeCounter++;
                            var textitem = new TextItem();
                            ParseCodeWrapper(textitem, snode);
                            spoilerContainer.TextItems.Add(textitem);
                            continue;
                        }
                        else if (snode.HasClass("post-bbcode-quote-wrapper"))
                        {
                            quoteCounter++;
                            var textitem = new TextItem();
                            ParseQuoteWrapper(textitem, snode);
                            spoilerContainer.TextItems.Add(textitem);
                            continue;
                        }
                    }
                    else if (snode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(snode.ParentNode.Name) && !string.IsNullOrWhiteSpace(snode.InnerText))
                    {
                        codeCounter = 0;
                        snode.InnerText.SplitIntoTextItems(spoilerContainer, TextKind.Text);
                    }
                }
            }

            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);

            if (spoilerContainerCollection.SpoilerContainers.LastOrDefault()?.TextItems?.All(x => x.Kind == TextKind.LineBreak) == true)
            {
                spoilerContainerCollection.SpoilerContainers.Remove(spoilerContainerCollection.SpoilerContainers.Last());
            }

            item.SpoilerContainerCollections.Add(spoilerContainerCollection);
        }

        private static void ParseQuoteWrapper(TextItem item, HtmlNode node)
        {
            QuoteTextContainer quoteContainer = new();

            List<string> itms = [];

            int codeCounter = 0;
            int spoilerCounter = 0;

            QuoteContainerCollection quoteContainerCollection = new()
            {
                UserName = node.SelectSingleNode(".//strong")?.InnerText?.Trim()
            };

            var clampedTextNode = node.SelectSingleNode(".//div[contains(@class, 'post-clamped-text')]") ?? node.SelectSingleNode(".//div[contains(@class, 'alt2 post-bbcode-quote')]");

            if (clampedTextNode != null)
            {
                foreach (var cnode in clampedTextNode.ChildNodes)
                {
                    if (cnode.NodeType is HtmlNodeType.Element)
                    {
                        if (cnode.Name == "br")
                        {
                            quoteContainerCollection.QuoteContainers.Add(quoteContainer);
                            quoteContainer = new();
                            quoteContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            if (codeCounter > 0 || spoilerCounter > 0)
                            {
                                quoteContainerCollection.QuoteContainers.Add(quoteContainer);
                                quoteContainer = new();
                                quoteContainer.TextItems.Add(new TextItem
                                {
                                    Kind = TextKind.LineBreak
                                });

                                codeCounter = 0;
                                spoilerCounter = 0;
                            }

                            quoteContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            continue;
                        }

                        if (htmlTags.Contains(cnode.Name))
                        {
                            if (cnode.Name == "a")
                            {
                                var url = cnode.Attributes["href"].Value;

                                if (url.StartsWith("/"))
                                {
                                    url = $"{flashbackUrl}{url}";
                                }

                                quoteContainer.TextItems.Add(new QuoteTextItem
                                {
                                    Text = HttpUtility.HtmlDecode(cnode.InnerText.Trim()) + " ",
                                    Kind = TextKind.Link,
                                    AdditionalData = url
                                });
                            }
                            else if (cnode.Name == "b")
                            {
                                cnode.InnerText.SplitIntoTextItems(quoteContainer, TextKind.Bold);
                            }
                            else if (cnode.Name == "i")
                            {
                                cnode.InnerText.SplitIntoTextItems(quoteContainer, TextKind.Italic);
                            }
                            else if (cnode.Name == "ul")
                            {
                                quoteContainerCollection.QuoteContainers.Add(quoteContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseUnorderedList(liTextContainers, cnode);

                                liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                                quoteContainerCollection.QuoteContainers.AddRange(liTextContainers);

                                quoteContainer = new QuoteTextContainer();
                            }
                            else if (cnode.Name == "ol")
                            {
                                quoteContainerCollection.QuoteContainers.Add(quoteContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseOrderedList(liTextContainers, cnode);

                                liTextContainers.Last().TextItems.Last().AdditionalData = "MarginBottom#10";

                                quoteContainerCollection.QuoteContainers.AddRange(liTextContainers);

                                quoteContainer = new QuoteTextContainer();
                            }
                            else if (cnode.Name == "img")
                            {
                                ParseSmiley(cnode, quoteContainer);

                                continue;
                            }
                        }
                        else if (cnode.HasClass("post-bbcode-spoiler-wrapper"))
                        {
                            spoilerCounter++;
                            var textItem = new QuoteTextItem();
                            ParseSpoilerWrapper(textItem, cnode);
                            quoteContainer.TextItems.Add(textItem);
                            continue;
                        }
                        else if (cnode.HasClass("post-bbcode-code-wrapper"))
                        {
                            codeCounter++;
                            var textItem = new QuoteTextItem();
                            ParseCodeWrapper(textItem, cnode);
                            quoteContainer.TextItems.Add(textItem);
                            continue;
                        }
                    }
                    else if (cnode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(cnode.ParentNode.Name) && !string.IsNullOrWhiteSpace(cnode.InnerText))
                    {
                        codeCounter = 0;
                        spoilerCounter = 0;

                        cnode.InnerText.SplitIntoTextItems(quoteContainer, TextKind.Text);
                    }
                }

                quoteContainerCollection.QuoteContainers.Add(quoteContainer);

                if (quoteContainerCollection.QuoteContainers.LastOrDefault()?.TextItems?.All(x => x.Kind == TextKind.LineBreak) == true)
                {
                    quoteContainerCollection.QuoteContainers.Remove(quoteContainerCollection.QuoteContainers.Last());
                }

                item.QuoteContainerCollections.Add(quoteContainerCollection);
            }
        }

        public static ThreadListPage ParseThreadListPage(HtmlDocument doc)
        {
            ThreadListPage threadListpage = new();

            var forumCategory = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(".//ol[contains(@class, 'breadcrumb')]//li[1]//a")?.InnerText);
            var forumName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(".//ol[contains(@class, 'breadcrumb')]//li[2]//a")?.InnerText);
            var subForumName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(".//ol[contains(@class, 'breadcrumb')]//li[3]//a")?.InnerText);
            threadListpage.ForumName = $"{forumCategory} / {forumName}{(subForumName != null ? $" / {subForumName}" : "")}";

            var threadsList = doc.DocumentNode.SelectSingleNode("//table[contains(@id, 'threadslist')]");

            var threadItems = new List<ThreadItem>();

            var counter = 1;

            foreach (var thread in threadsList.SelectNodes(".//tbody//tr"))
            {
                var topic = new ThreadItem
                {
                    Index = counter++,
                    TopicName = (thread.HasClass("tr_sticky") ? "Viktigt: " : "") + HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[2]//div//a")?.InnerText?.Trim()),
                    TopicUrl = flashbackUrl + HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[2]//div//a")?.GetAttributeValue("href", null)),
                    Author = HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[contains(@class, 'td_title')]//span//span")?.InnerText?.Trim()),
                    PostDate = HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[contains(@class, 'td_last_post')]//div")?.InnerText?.Trim()),
                    LastPostUserName = HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[contains(@class, 'td_last_post')]//div[2]//span//a")?.InnerText?.Trim()),
                    NumReplies = HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[3]//div[1]")?.InnerText?.Trim()),
                    NumViews = HttpUtility.HtmlDecode(thread.SelectSingleNode(".//td[3]//div[2]")?.InnerText?.Trim()),
                    PinnedThread = thread.HasClass("tr_sticky")
                };

                threadItems.Add(topic);
            }

            threadListpage.ThreadItems.AddRange(threadItems);

            var forumsListNode = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'forumslist')]");

            if (forumsListNode != null)
            {
                foreach (var forumNode in forumsListNode.SelectNodes(".//tr"))
                {
                    var forum = new ForumItem
                    {
                        ForumName = HttpUtility.HtmlDecode(forumNode.SelectSingleNode(".//a//strong")?.InnerText?.Trim()),
                        ForumUrl = HttpUtility.HtmlDecode(flashbackUrl + forumNode.SelectSingleNode(".//a").Attributes["href"].Value?.Trim()),
                        TopicName = HttpUtility.HtmlDecode(forumNode.SelectSingleNode(".//td[2]//a//strong")?.InnerText?.Trim()),
                        TopicUrl = HttpUtility.HtmlDecode(flashbackUrl + forumNode.SelectSingleNode(".//td[2]//a").Attributes["href"].Value?.Trim()),
                        LastPostUserName = HttpUtility.HtmlDecode(forumNode.SelectSingleNode(".//td[2]//div//div[2]//a")?.InnerText?.Trim()),
                        LastPostDate = HttpUtility.HtmlDecode(forumNode.SelectSingleNode(".//td[2]//div//div[2]//a")?.NextSibling?.InnerText?.Trim())
                    };

                    threadListpage.SubForums.Add(forum);
                }
            }

            return threadListpage;
        }
    }
}