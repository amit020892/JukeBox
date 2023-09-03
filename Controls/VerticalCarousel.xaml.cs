using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for VerticalCarousel.xaml
    /// </summary>
    public partial class VerticalCarousel : UserControl
    {
        public VerticalCarousel()
        {
            InitializeComponent();
        }
        private int currentElement = 0;

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if (currentElement < 2)
            {
                currentElement++;
                AnimateCarousel();
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (currentElement > 0)
            {
                currentElement--;
                AnimateCarousel();
            }
        }

        private void AnimateCarousel()
        {
            Storyboard storyboard = (this.Resources["CarouselStoryboard"] as Storyboard);
            DoubleAnimation animation = storyboard.Children.First() as DoubleAnimation;
            animation.To = -this.Width * currentElement;
            storyboard.Begin();
        }

        internal void AddChildren(UniformGrid wrapPanel)
        {
            Carousel.Children.Add(wrapPanel);
        }
    }
}
