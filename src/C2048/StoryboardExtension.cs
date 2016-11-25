using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace C2048
{
    public static class StoryboardExtension
    {
        public static Task<bool> BeginAsync(this Storyboard b, bool result)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            b.Completed += (s, e) => { tcs.SetResult(result); };

            if (b.Children.Count > 0) { b.Begin(); }
            else { tcs.SetResult(result); }

            return tcs.Task;
        }
    }
}
