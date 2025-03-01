using System.ComponentModel;

namespace Services.Enum;

public enum OrderShopStatusEnum
{
    [Description("SystemChanged at first - Core")]
    WaitForPayment = 1,

    [Description("SystemChanged after payment successfully - Core")]
    Pending,

    [Description("Shipping - ShopChanged after Approved - Core")]
    Shipping,

    [Description("Received - CustomerChanged after Shipping - Core")]
    Received,

    [Description("Returning - CustomerChanged after Received - Core")]
    Returning,

    [Description("Returned - ShopChanged after Returning - Core")]
    Returned,

    [Description("Returned with damage - ShopChanged after Returned - Sub")]
    ReturnedWithDamage,

    [Description("Waiting for refund - ShopChanged after Returned - Core")]
    WaitingForRefund,

    [Description("Completed - AdminChanged after WaitingForRefund - Core")]
    Completed,

    [Description("Rejected - ShopChanged after Pending - Sub")]
    Rejected,

    [Description("Cancelled - CustomerChanged after WaitForPayment - Sub")]
    Cancelled
}