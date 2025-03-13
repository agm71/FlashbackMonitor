using HtmlAgilityPack;
using System.Linq;
using System.Web;
using FlashbackMonitor.Services;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;

namespace FlashbackMonitor.Utils
{
    public class FlashbackParser
    {
        private static TopicPage topicsPage = new();
        private static readonly string[] htmlTags = ["a", "b", "i", "ul", "li"];
        private static readonly string flashbackUrl = "https://www.flashback.org";

        public static TopicPage ParseTopicsPage(HtmlDocument doc)
        {
            var TopicTitle = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//meta[contains(@property, 'og:title')]").GetAttributeValue("content", null));

            topicsPage.TopicName = TopicTitle;
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

                var postDateRaw = postN.SelectSingleNode(".//div[contains(@class, 'post-heading')]")?.InnerText?.Trim();

                var postDate = FlashbackRegexes.PostDateRegex().Match(postDateRaw)?.Value;

                FlashbackPostItem post = new()
                {
                    Author = postRow.SelectSingleNode(".//a[contains(@class, 'post-user-username')]")?.InnerText?.Trim(),
                    UserRegistration = postRow.SelectSingleNode(".//div[contains(@class, 'post-user-info')]//div")?.InnerText?.Trim(),
                    UserPosts = HttpUtility.HtmlDecode(postRow.SelectSingleNode(".//div[contains(@class, 'post-user-info')]//div[2]")?.InnerText?.Trim()),
                    PostDate = HttpUtility.HtmlDecode(postDate),
                };

                var textContainer = new TextContainer();

                foreach (var node in postRow.SelectSingleNode(".//div[contains(@class, 'post_message')]")?.ChildNodes)
                {
                    if (node.NodeType == HtmlNodeType.Element)
                    {
                        if (node.Name == "br")
                        {
                            textContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.LineBreak
                            });
                            post.TextContainers.Add(textContainer);
                            textContainer = new TextContainer();
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
                                    Text = HttpUtility.HtmlDecode(node.InnerText.Trim()),
                                    Kind = TextKind.Link,
                                    AdditionalData = url
                                });
                            }
                            else if (node.Name == "b")
                            {
                                textContainer.TextItems.Add(new TextItem
                                {
                                    Kind = TextKind.Bold,
                                    Text = HttpUtility.HtmlDecode(node.InnerText.Trim()) + " "
                                });
                            }
                            else if (node.Name == "i")
                            {
                                textContainer.TextItems.Add(new TextItem
                                {
                                    Kind = TextKind.Italic,
                                    Text = HttpUtility.HtmlDecode(node.InnerText.Trim()) + " "
                                });
                            }
                        }

                        if (node.HasClass("post-bbcode-quote-wrapper"))
                        {
                            post.TextContainers.Add(textContainer);

                            textContainer = new TextContainer();
                            var textitem = new TextItem();
                            ParseQuoteWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);
  
                            post.TextContainers.Add(textContainer);
                            
                            textContainer = new TextContainer();

                            continue;
                        }

                        if (node.HasClass("post-bbcode-spoiler-wrapper"))
                        {
                            post.TextContainers.Add(textContainer);

                            textContainer = new TextContainer();
                            var textitem = new TextItem();
                            ParseSpoilerWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);

                            post.TextContainers.Add(textContainer);

                            textContainer = new TextContainer();
                        }

                        if (node.Name == "ul")
                        {
                            post.TextContainers.Add(textContainer);

                            List<ITextContainer> liTextContainers = [];
                            
                            ParseUnorderedList(liTextContainers, node);

                            post.TextContainers.AddRange(liTextContainers);

                            post.TextContainers.Add(new TextContainer
                            {
                                TextItems = new List<ITextItem>
                                    {
                                        new TextItem
                                        {
                                            Kind = TextKind.LineBreak
                                        }
                                    }
                            });

                            textContainer = new TextContainer();
                        }
                    }
                    else if (node.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(node.ParentNode.Name) && !string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        node.InnerText.SplitIntoTextItems(textContainer, TextKind.Text);
                    }
                }

                post.TextContainers.Add(textContainer);
                topicsPage.PostItems.Add(post);
            }

            return topicsPage;
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
            SpoilerContainerCollection spoilerContainerCollection = new();

            var spoilerContentNodes = node.SelectNodes(".//div[contains(@class, 'alt2 post-bbcode-spoiler hidden')]");

            if (spoilerContentNodes != null)
            {
                foreach (var snode in spoilerContentNodes.SelectMany(x => x.ChildNodes))
                {
                    ITextItem spoilerTextItem = null;

                    if (snode.NodeType is HtmlNodeType.Element)
                    {
                        if (snode.Name == "br")
                        {
                            spoilerContainer.TextItems.Add(new SpoilerTextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
                            spoilerContainer = new SpoilerTextContainer();
                            continue;
                        }

                        if (htmlTags.Contains(snode.Name))
                        {
                            spoilerTextItem = new SpoilerTextItem
                            {
                                Text = HttpUtility.HtmlDecode(snode.InnerText.Trim()) + " "
                            };

                            if (snode.Name == "a")
                            {
                                spoilerTextItem.Kind = TextKind.Link;

                                var url = snode.Attributes["href"].Value;

                                if (url.StartsWith("/"))
                                {
                                    url = $"{flashbackUrl}{url}";
                                }

                                spoilerTextItem.AdditionalData = url;
                            }
                            else if (snode.Name == "b")
                            {
                                spoilerTextItem.Kind = TextKind.Bold;
                            }
                            else if (snode.Name == "i")
                            {
                                spoilerTextItem.Kind = TextKind.Italic;
                            }
                            else if (snode.Name == "ul")
                            {
                                spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseUnorderedList(liTextContainers, node);
                                spoilerContainerCollection.SpoilerContainers.AddRange(liTextContainers);

                                spoilerContainerCollection.SpoilerContainers.Add(new SpoilerTextContainer
                                {
                                    TextItems =
                                    [
                                        new SpoilerTextItem
                                        {
                                            Kind = TextKind.LineBreak
                                        }
                                    ]
                                });

                                spoilerContainer = new SpoilerTextContainer();

                                continue;
                            }

                            spoilerContainer.TextItems.Add(spoilerTextItem);
                        }
                    }
                    else if (snode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(snode.ParentNode.Name) && !string.IsNullOrWhiteSpace(snode.InnerText))
                    {
                        snode.InnerText.SplitIntoTextItems(spoilerContainer, TextKind.Text);
                    }
                }
            }

            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
            item.SpoilerContainerCollections.Add(spoilerContainerCollection);
        }

        private static void ParseQuoteWrapper(TextItem item, HtmlNode node)
        {
            QuoteTextContainer quoteContainer = new();

            List<string> itms = [];

            QuoteContainerCollection quoteContainerCollection = new()
            {
                UserName = node.SelectSingleNode(".//strong")?.InnerText?.Trim()
            };

            var clampedTextNode = node.SelectSingleNode(".//div[contains(@class, 'post-clamped-text')]") ?? node.SelectSingleNode(".//div[contains(@class, 'alt2 post-bbcode-quote')]");

            if (clampedTextNode != null)
            {
                foreach (var cnode in clampedTextNode.ChildNodes)
                {
                    QuoteTextItem quoteTextItem = null;
                    if (cnode.NodeType is HtmlNodeType.Element)
                    {
                        if (cnode.Name == "br")
                        {
                            quoteContainer.TextItems.Add(new QuoteTextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            quoteContainerCollection.QuoteContainers.Add(quoteContainer);

                            quoteContainer = new QuoteTextContainer();

                            continue;
                        }

                        if (htmlTags.Contains(cnode.Name))
                        {
                            quoteTextItem = new QuoteTextItem
                            {
                                Text = HttpUtility.HtmlDecode(cnode.InnerText.Trim()) + " "
                            };

                            if (cnode.Name == "a")
                            {
                                quoteTextItem.Kind = TextKind.Link;

                                var url = cnode.Attributes["href"].Value;

                                if (url.StartsWith("/"))
                                {
                                    url = $"{flashbackUrl}{url}";
                                }

                                quoteTextItem.AdditionalData = url;
                            }
                            else if (cnode.Name == "b")
                            {
                                quoteTextItem.Kind = TextKind.Bold;
                            }
                            else if (cnode.Name == "i")
                            {
                                quoteTextItem.Kind = TextKind.Italic;
                            }
                            else if (cnode.Name == "ul")
                            {
                                quoteContainerCollection.QuoteContainers.Add(quoteContainer);

                                List<ITextContainer> liTextContainers = [];

                                ParseUnorderedList(liTextContainers, node);
                                quoteContainerCollection.QuoteContainers.AddRange(liTextContainers);

                                quoteContainerCollection.QuoteContainers.Add(new QuoteTextContainer
                                {
                                    TextItems =
                                    [
                                        new QuoteTextItem
                                        {
                                            Kind = TextKind.LineBreak
                                        }
                                    ]
                                });

                                quoteContainer = new QuoteTextContainer();

                                continue;
                            }

                            quoteContainer.TextItems.Add(quoteTextItem);
                        }
                        else if (cnode.HasClass("post-bbcode-spoiler-wrapper"))
                        {
                            quoteContainer ??= new QuoteTextContainer();
                            var it = quoteContainer.TextItems.LastOrDefault() ?? new QuoteTextItem();
                            ParseSpoilerWrapper(quoteContainerCollection.QuoteContainers.LastOrDefault()?.TextItems?.LastOrDefault() ?? it, cnode);
                            quoteContainer.TextItems.Add(it);
                        }
                    }
                    else if (cnode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(cnode.ParentNode.Name) && !string.IsNullOrWhiteSpace(cnode.InnerText))
                    {
                        itms = cnode?.InnerText.Trim().Split(" ").ToList();
                        for (int i = 0; i < itms.Count; i++)
                        {
                            quoteContainer.TextItems.Add(new QuoteTextItem
                            {
                                Kind = TextKind.Text,
                                Text = HttpUtility.HtmlDecode(itms[i].Trim()) + " "
                            });
                        }
                    }
                }
            }

            quoteContainerCollection.QuoteContainers.Add(quoteContainer);
            item.QuoteContainerCollections.Add(quoteContainerCollection);
        }
    }
}