using PCL.Neo.Animations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL.Neo.Helpers;

/// <summary>
/// 动画帮助类，用来同时执行不同动画。
/// </summary>
public class AnimationHelper(List<IAnimation> animations)
{
    public List<IAnimation> Animations { get; set; } = animations;
    public List<Task> Tasks { get; } = [];
    public bool Loop { get; set; } = false;

    public AnimationHelper() : this([]){}

    public void Run()
    {
        _ = RunAsync();
    }

    public async Task RunAsync()
    {
        Tasks.Clear();

        do
        {
            Tasks.Clear();
            await RunAsyncCore();
        } while (Loop);
    }

    private async Task RunAsyncCore()
    {
        // 根据 Wait 进行动画分组
        var groupedAnimations = new List<List<IAnimation>>();
        var currentGroup = new List<IAnimation>();

        foreach (IAnimation animation in Animations)
        {
            if (animation.Wait)
            {
                if (currentGroup.Count > 0)
                {
                    groupedAnimations.Add([..currentGroup]);
                    currentGroup.Clear();
                    continue;
                }
            }

            currentGroup.Add(animation);
        }

        if (currentGroup.Count > 0)
        {
            groupedAnimations.Add([..currentGroup]);
        }

        currentGroup.Clear();

        foreach (List<IAnimation> list in groupedAnimations)
        {
            foreach (IAnimation animation in list)
            {
                Tasks.Add(animation.RunAsync());
            }

            await Task.WhenAll(Tasks);
        }
    }

    public void Cancel()
    {
        foreach (IAnimation animation in Animations)
        {
            animation.Cancel();
        }
    }

    public void CancelAndClear()
    {
        Cancel();
        Animations.Clear();
    }
}