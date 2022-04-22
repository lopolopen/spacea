using System.ComponentModel.DataAnnotations;

namespace SpaceA.Model
{
    public enum WorkItemType
    {
        Story,
        Task,
        Bug,
        TestSuite
    }

    public enum WorkItemPriority
    {
        High,
        Normal,
        Low
    }

    public enum WorkItemState
    {
        New,
        Active,
        Resolved,
        Prepared,
        Accepted,
        Closed,
        Removed
    }

    public enum Severity
    {
        [Display(Name = "阻塞")]
        Blocker,

        [Display(Name = "严重")]
        Critical,

        [Display(Name = "高")]
        High,

        [Display(Name = "中")]
        Medium,

        [Display(Name = "低")]
        Low
    }

    public enum CapacityType
    {
        [Display(Name = "未分配")]
        Unassigned,
        [Display(Name = "开发")]
        Development,
        [Display(Name = "设计")]
        Design,
        [Display(Name = "文档")]
        Documentation,
        [Display(Name = "需求")]
        Requirement,
        [Display(Name = "测试")]
        Testing
    }

    public enum AccessLevel
    {
        NoOne = 0,
        Developer = 30,
        Maintainer = 40,
        Admin = 60
    }

    public enum Location
    {
        Top,
        // Current,
        Bottom
    }
}