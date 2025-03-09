using HtmlAgilityPack;
using System.Linq;
using System.Web;
using FlashbackMonitor.Services;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace FlashbackMonitor.Utils
{
    public class FlashbackParser
    {
        private static TopicPage topicsPage = new();
        private static readonly string[] htmlTags = ["a", "b", "i"];
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
            topicsPage.Pages.Clear();

            if (pagerNode != null)
            {
                topicsPage.CurrentPage = pagerNode.GetAttributeValue("data-page", 1);

                var pageCount = Convert.ToInt32(pagerNode.GetAttributeValue("data-total-pages", 1));

                for (int i = 1; i <= pageCount; i++)
                {
                    topicsPage.Pages.Add(i.ToString());
                }
            }
            else
            {
                topicsPage.Pages.Add("1");
                topicsPage.CurrentPage = 1;
            }
            
            foreach (HtmlNode postN in postNodes)
            {
                var postRow = postN.SelectSingleNode(".//div[contains(@class, 'post-row')]");

                var postDateRaw = postN.SelectSingleNode(".//div[contains(@class, 'post-heading')]")?.InnerText?.Trim();

                var postDate = Regex.Match(postDateRaw, "^[^\r\n\t]*").Value;
                
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

                            var onlyTextSiblingsBefore = AreOnlyTextSiblingsBefore(node);
                            if (onlyTextSiblingsBefore)
                            {
                                var tc = new TextContainer();
                                tc.TextItems.Add(new TextItem
                                {
                                    Kind = TextKind.LineBreak
                                });
                                post.TextContainers.Add(tc);
                            }

                            textContainer = new TextContainer();
                            var textitem = new TextItem();
                            ParseQuoteWrapper(textitem, node);
                            textContainer.TextItems.Add(textitem);
  
                            post.TextContainers.Add(textContainer);
                            
                            textContainer = new TextContainer();
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
                    }
                    else if (node.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(node.ParentNode.Name) && !string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        itms = node?.InnerText.Trim().Split(" ").ToList();
                        for (int i = 0; i < itms.Count; i++)
                        {
                            textContainer.TextItems.Add(new TextItem
                            {
                                Kind = TextKind.Text,
                                Text = HttpUtility.HtmlDecode(itms[i].Trim()) + " "
                            });
                        }
                    }
                }

                post.TextContainers.Add(textContainer);
                topicsPage.PostItems.Add(post);
            }

            return topicsPage;
        }

        static bool AreOnlyTextSiblingsBefore(HtmlNode targetNode)
        {
            var previousSibling = targetNode.PreviousSibling;

            while (previousSibling != null)
            {
                if (previousSibling.NodeType != HtmlNodeType.Element && !string.IsNullOrWhiteSpace(previousSibling.InnerText))
                {
                    previousSibling = previousSibling.PreviousSibling;
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static void ParseSpoilerWrapper(dynamic item, HtmlNode node)
        {
            SpoilerContainer spoilerContainer = new();
            List<string> itms = [];
            SpoilerContainerCollection spoilerContainerCollection = new();

            var spoilerContentNodes = node.SelectNodes(".//div[contains(@class, 'alt2 post-bbcode-spoiler hidden')]");

            if (spoilerContentNodes != null)
            {
                foreach (var snode in spoilerContentNodes.SelectMany(x => x.ChildNodes))
                {
                    SpoilerTextItem spoilerTextItem = null;

                    if (snode.NodeType is HtmlNodeType.Element)
                    {
                        if (snode.Name == "br")
                        {
                            spoilerContainer.SpoilerTextItems.Add(new SpoilerTextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
                            spoilerContainer = new SpoilerContainer();
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

                            spoilerContainer.SpoilerTextItems.Add(spoilerTextItem);
                        }
                    }
                    else if (snode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(snode.ParentNode.Name) && !string.IsNullOrWhiteSpace(snode.InnerText))
                    {
                        itms = snode?.InnerText.Trim().Split(" ").ToList();
                        for (int i = 0; i < itms.Count; i++)
                        {
                            spoilerContainer.SpoilerTextItems.Add(new SpoilerTextItem
                            {
                                Kind = TextKind.Text,
                                Text = HttpUtility.HtmlDecode(itms[i].Trim()) + " "
                            });
                        }
                    }
                }
            }

            spoilerContainerCollection.SpoilerContainers.Add(spoilerContainer);
            item.SpoilerContainerCollections.Add(spoilerContainerCollection);
        }

        private static void ParseQuoteWrapper(TextItem item, HtmlNode node)
        {
            QuoteContainer quoteContainer = new();
            
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
                            quoteContainer.QuoteTextItems.Add(new QuoteTextItem
                            {
                                Kind = TextKind.LineBreak
                            });

                            quoteContainerCollection.QuoteContainers.Add(quoteContainer);
                            
                            quoteContainer = new QuoteContainer();
                            
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

                            quoteContainer.QuoteTextItems.Add(quoteTextItem);
                        }
                        else if (cnode.HasClass("post-bbcode-spoiler-wrapper"))
                        {
                            quoteContainer ??= new QuoteContainer();
                            var it = quoteContainer.QuoteTextItems.LastOrDefault() ?? new QuoteTextItem();
                            ParseSpoilerWrapper(quoteContainerCollection.QuoteContainers.LastOrDefault()?.QuoteTextItems?.LastOrDefault() ?? it, cnode);
                            quoteContainer.QuoteTextItems.Add(it);
                        }
                    }
                    else if (cnode.NodeType == HtmlNodeType.Text
                        && !htmlTags.Contains(cnode.ParentNode.Name) && !string.IsNullOrWhiteSpace(cnode.InnerText))
                    {
                        itms = cnode?.InnerText.Trim().Split(" ").ToList();
                        for (int i = 0; i < itms.Count; i++)
                        {
                            quoteContainer.QuoteTextItems.Add(new QuoteTextItem
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