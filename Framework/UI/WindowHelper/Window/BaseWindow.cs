using FairyGUI;

public class BaseWindow : GComponent, IWindow
{
    public void show()
    {
        this.visible = true;
        this.onShow();
    }

    public void close()
    {
        this.visible = false;
        this.onClose();
    }

    public void onShow()
    {

    }

    public void onClose()
    {
        
    }
}