using FlashbackMonitor.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlashbackMonitor.Services
{
    #region Interfaces
    public interface ITextContainer
    {
        List<ITextItem> TextItems { get; set; }
    }

    public interface ITextItem
    {
        string Text { get; set; }
        TextKind Kind { get; set; }
        string AdditionalData { get; set; }
        List<QuoteContainerCollection> QuoteContainerCollections { get; set; }
        List<SpoilerContainerCollection> SpoilerContainerCollections { get; set; }
    }
    #endregion

    public class TopicPage : ViewModelBase
    {
        private string _topicName;
        public string TopicName
        {
            get => _topicName;
            set => this.RaiseAndSetIfChanged(ref _topicName, value);
        }

        public List<string> PageNumbers { get; set; } = [];

        public int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public ObservableCollection<FlashbackPostItem> PostItems { get; set; } = [];
    }

    public class ThreadListPage : ViewModelBase
    {
        private string _forumName;
        public string ForumName
        {
            get => _forumName;
            set => this.RaiseAndSetIfChanged(ref _forumName, value);
        }

        public ObservableCollection<ThreadItem> ThreadItems { get; set; } = [];
    }

    public class ThreadItem
    {
        public string Author { get; set; }
        public string LastPostUserName { get; set; }
        public string TopicUrl { get; set; }
        public string TopicName { get; set; }
        public string PostDate { get; set; }
        public string NumReplies { get; set; }
        public string NumViews { get; set; }
        public bool PinnedThread { get; set; }
    }

    public class FlashbackPostItem
    {
        public string Author { get; set; }
        public string UserAvatar { get; set; }
        public string UserRegistration { get; set; }
        public string UserPosts { get; set; }
        public string PostDate { get; set; }
        public string UserType { get; set; }
        public string OnlineStatus { get; set; }
        public string OnlineStatusColor { get; set; }
        public List<ITextContainer> TextContainers { get; set; } = [];
    }

    public class TextItem : ITextItem
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
        public List<ITextContainer> QuoteContainers { get; set; } = [];
    }

    public class SpoilerContainerCollection
    {
        public List<ITextContainer> SpoilerContainers { get; set; } = [];
    }

    public class QuoteTextContainer : ITextContainer
    {
        public List<ITextItem> TextItems { get; set; } = [];
    }

    public class TextContainer : ITextContainer
    {
        public List<ITextItem> TextItems { get; set; } = [];
    }

    public class QuoteTextItem : ITextItem
    {
        public string Text { get; set; }
        public TextKind Kind { get; set; }
        public string AdditionalData { get; set; }
        public List<SpoilerContainerCollection> SpoilerContainerCollections { get; set; } = [];
        public List<QuoteContainerCollection> QuoteContainerCollections { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }

    public class SpoilerTextContainer : ITextContainer
    {
        public List<ITextItem> TextItems { get; set; } = [];
    }

    public class SpoilerTextItem : ITextItem
    {
        public string Text { set; get; }
        public TextKind Kind { get; set; }
        public string AdditionalData { get; set; }
        public List<QuoteContainerCollection> QuoteContainerCollections { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public List<SpoilerContainerCollection> SpoilerContainerCollections { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }

    public enum TextKind
    {
        Text,
        Link,
        Italic,
        Bold,
        LineBreak,
        Bullet
    }
}