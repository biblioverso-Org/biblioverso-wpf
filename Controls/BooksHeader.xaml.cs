using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace library.Controls
{
    public partial class BooksHeader : UserControl
    {
        public BooksHeader()
        {
            InitializeComponent();
        }

        // === Commands ===
        public static readonly DependencyProperty AddBookCommandProperty =
            DependencyProperty.Register(nameof(AddBookCommand), typeof(ICommand), typeof(BooksHeader));

        public ICommand? AddBookCommand
        {
            get => (ICommand?)GetValue(AddBookCommandProperty);
            set => SetValue(AddBookCommandProperty, value);
        }

        public static readonly DependencyProperty GenerateReportCommandProperty =
            DependencyProperty.Register(nameof(GenerateReportCommand), typeof(ICommand), typeof(BooksHeader));

        public ICommand? GenerateReportCommand
        {
            get => (ICommand?)GetValue(GenerateReportCommandProperty);
            set => SetValue(GenerateReportCommandProperty, value);
        }

        // === Availability (string o enum) ===
        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register(nameof(Availability), typeof(string), typeof(BooksHeader), new PropertyMetadata("all"));

        public string Availability
        {
            get => (string)GetValue(AvailabilityProperty);
            set => SetValue(AvailabilityProperty, value);
        }

        // === SelectedGenre ===
        public static readonly DependencyProperty SelectedGenreProperty =
            DependencyProperty.Register(nameof(SelectedGenre), typeof(string), typeof(BooksHeader), new PropertyMetadata("all"));

        public string SelectedGenre
        {
            get => (string)GetValue(SelectedGenreProperty);
            set => SetValue(SelectedGenreProperty, value);
        }

        // === MinRating ===
        public static readonly DependencyProperty MinRatingProperty =
            DependencyProperty.Register(nameof(MinRating), typeof(double), typeof(BooksHeader), new PropertyMetadata(0.0));

        public double MinRating
        {
            get => (double)GetValue(MinRatingProperty);
            set => SetValue(MinRatingProperty, value);
        }
    }
}
