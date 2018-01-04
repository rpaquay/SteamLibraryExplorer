using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SteamLibraryExplorer.WpfUtils {
  public class ListViewColumnSorter {
    private GridViewColumnHeader _lastSortedColumn;
    private SortAdorner _listViewSortAdorner;

    public void SortColumn(List<ListView> listView, GridViewColumnHeader header) {
      string propertyNameSortBy = header.Tag.ToString();
      if (_lastSortedColumn != null) {
        AdornerLayer.GetAdornerLayer(_lastSortedColumn).Remove(_listViewSortAdorner);
        listView.ForEach(x => x.Items.SortDescriptions.Clear());
      }

      ListSortDirection newDir = ListSortDirection.Ascending;
      if (ReferenceEquals(_lastSortedColumn, header) && _listViewSortAdorner.Direction == newDir)
        newDir = ListSortDirection.Descending;

      _lastSortedColumn = header;
      _listViewSortAdorner = new SortAdorner(_lastSortedColumn, newDir);
      AdornerLayer.GetAdornerLayer(_lastSortedColumn).Add(_listViewSortAdorner);
      listView.ForEach(x => x.Items.SortDescriptions.Add(new SortDescription(propertyNameSortBy, newDir)));
    }

    public class SortAdorner : Adorner {
      private static readonly Geometry AscGeometry = Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");
      private static readonly Geometry DescGeometry = Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

      public SortAdorner(UIElement element, ListSortDirection dir)
        : base(element) {
        Direction = dir;
      }

      public ListSortDirection Direction { get; private set; }

      protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);

        if (AdornedElement.RenderSize.Width < 20)
          return;

        TranslateTransform transform = new TranslateTransform
        (
          AdornedElement.RenderSize.Width - 15,
          (AdornedElement.RenderSize.Height - 5) / 2
        );
        drawingContext.PushTransform(transform);

        Geometry geometry = AscGeometry;
        if (Direction == ListSortDirection.Descending)
          geometry = DescGeometry;
        drawingContext.DrawGeometry(Brushes.Black, null, geometry);

        drawingContext.Pop();
      }
    }
  }
}