using FlashbackMonitor.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlashbackMonitor.Services
{
    public class TopicPage : ViewModelBase
    {
        public ObservableCollection<FlashbackPostItem> PostItems { get; set; } = [];

        private string _topicName;
        public string TopicName
        {
            get => _topicName;
            set => this.RaiseAndSetIfChanged(ref _topicName, value);
        }

        public int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }
        public List<string> Pages { get; set; } = [];
    }

    public class FlashbackPostItem
    {
        public string Author { get; set; }
        public string UserRegistration { get; set; }
        public string UserPosts { get; set; }
        public string PostDate { get; set; }
        public List<TextItem> TextItems { get; set; } = [];
        public List<TextContainer> TextContainers { get; set; } = [];
    }

    public class TextItem
    {
        public string Text { get; set; }
        public TextKind Kind { get; set; }
        public string AdditionalData { get; set; }
        public List<QuoteContainerCollection> QuoteContainerCollections { get; set; } = [];
        public List<SpoilerContainerCollection> SpoilerContainerCollections { get; set; } = [];
    }

    public class QuoteContainerCollection
    {
        public string UserName { get; set; }
        public List<QuoteContainer> QuoteContainers { get; set; } = [];
    }

    public class SpoilerContainerCollection
    {
        public List<SpoilerContainer> SpoilerContainers { get; set; } = [];
    }

    public class QuoteContainer
    {
        public List<QuoteTextItem> QuoteTextItems { get; set; } = [];
    }

    public class TextContainer
    {
        public List<TextItem> TextItems { get; set; } = [];
    }

    public class QuoteTextItem
    {
        public string Text { get; set; }
        public TextKind Kind { get; set; }
        public string AdditionalData { get; set; }
        public List<SpoilerContainer> Spoilers { get; set; } = [];
        public List<SpoilerContainerCollection> SpoilerContainerCollections { get; set; } = [];
    }

    public class SpoilerContainer
    {
        public List<SpoilerTextItem> SpoilerTextItems { get; set; } = [];
    }

    public class SpoilerTextItem
    {
        public string Text { set; get; }
        public TextKind Kind { get; set; }
        public string AdditionalData { get; set; }
    }

    public enum TextKind
    {
        Text,
        Link,
        Italic,
        Bold,
        LineBreak
    }
}