using FlashbackMonitor.Services;
using System.Linq;
using System.Web;

namespace FlashbackMonitor.Utils
{
    public static class ExtensionMethods
    {
        public static void SplitIntoTextItems(this string innerText, ITextContainer textContainer, TextKind textKind, string additionalData = null)
        {
            var textParts = innerText.Trim().Split(" ").ToList();

            for (int i = 0; i < textParts.Count; i++)
            {
                textContainer.TextItems.Add(new TextItem
                {
                    Kind = textKind,
                    Text = HttpUtility.HtmlDecode(textParts[i].Trim()) + (textParts[i] == "&quot;" ? "" : " "),
                    AdditionalData = additionalData
                });
            }
        }
    }
}