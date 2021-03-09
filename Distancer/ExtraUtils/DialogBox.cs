using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Content.Res.Resources;

public class Show_Dialog
{
    public enum MessageResult
    {
        NONE = 0,
        OK = 1,
        CANCEL = 2,
        ABORT = 3,
        RETRY = 4,
        IGNORE = 5,
        YES = 6,
        NO = 7
    }

    Activity mcontext;
    public Show_Dialog(Activity activity) : base()
    {
        this.mcontext = activity;
    }

    public Task<MessageResult> ShowDialog(string Title, string Message, bool SetCancelable = false, bool SetInverseBackgroundForced = false, MessageResult PositiveButton = MessageResult.OK, MessageResult NegativeButton = MessageResult.NONE, MessageResult NeutralButton = MessageResult.NONE, int IconAttribute = Android.Resource.Attribute.AlertDialogIcon)
    {
        var tcs = new TaskCompletionSource<MessageResult>();

        Theme appTheme = mcontext.ApplicationContext.Theme;
        var builder = new AlertDialog.Builder(new ContextThemeWrapper(mcontext, appTheme));

        //var builder = new AlertDialog.Builder(mcontext);
        builder.SetIconAttribute(IconAttribute);
        builder.SetTitle(Title);
        builder.SetMessage(Message);
        //builder.SetInverseBackgroundForced(SetInverseBackgroundForced);
        builder.SetCancelable(SetCancelable);

        builder.SetPositiveButton((PositiveButton != MessageResult.NONE) ? PositiveButton.ToString() : string.Empty, (senderAlert, args) =>
        {
            tcs.SetResult(PositiveButton);
        });
        builder.SetNegativeButton((NegativeButton != MessageResult.NONE) ? NegativeButton.ToString() : string.Empty, delegate
        {
            tcs.SetResult(NegativeButton);
        });
        builder.SetNeutralButton((NeutralButton != MessageResult.NONE) ? NeutralButton.ToString() : string.Empty, delegate
        {
            tcs.SetResult(NeutralButton);
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            builder.Show();
        });

        return tcs.Task;
    }
}

//if (MainThread.IsMainThread)
//{
//    // Code to run if this is the main thread
//}
//else
//{
//    // Code to run if this is a secondary thread
//}
//if (MainThread.IsMainThread)
//{
//    MyMainThreadCode();
//}
//else
//{
//    MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
//}