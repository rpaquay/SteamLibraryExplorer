using System.Windows.Data;

namespace SteamLibraryExplorer.ViewModel {
  public class MyCollectionViewGroup : CollectionViewGroup {
    public MyCollectionViewGroup() : base("Foo") {
    }

    public override bool IsBottomLevel { get { return true; } }
  }
}
