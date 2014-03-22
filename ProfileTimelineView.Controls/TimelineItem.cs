using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ProfileTimelineView.Controls
{
    public class TimelineItem : Control
    {
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Body
        {
            get { return (string)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TimelineItem), new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TimelineItem), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(string), typeof(TimelineItem), new PropertyMetadata(default(string)));

        public TimelineItem()
        {
            DefaultStyleKey = typeof (TimelineItem);
        }
    }
}
