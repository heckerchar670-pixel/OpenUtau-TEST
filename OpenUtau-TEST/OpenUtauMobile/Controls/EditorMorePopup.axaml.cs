using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;

namespace OpenUtauMobile.Controls;

public partial class EditorMorePopup : PopupDialogControl
{
    protected override PopupDialogWidthPreset WidthPreset => PopupDialogWidthPreset.Regular;

    public EditorMorePopup()
    {
        InitializeComponent();
        // OldUI 主题显示旧版 master 的菜单，其他主题保留 dev 选项卡
        bool oldUi = LunaiThemes.Current == LunaiThemes.Master;
        DevTabs.IsVisible = !oldUi;
        OldTabs.IsVisible = oldUi;
    }
}
