namespace TwoHandApp.Models;

public enum Permission
{
    Ads_Create,
    Ads_Edit_Own,
    Ads_Delete_Own,
    Ads_View,
    Ads_Report,

    Ads_Edit_Any,
    Ads_Delete_Any,
    Ads_Approve,
    Ads_Highlight,
    Categories_Manage,
    Cities_Manage,

    Users_View,
    Users_Ban,
    Users_Unban,
    Users_ManageRoles,

    Comments_View,
    Comments_Delete,
    Reports_View,
    Reports_Resolve,

    Roles_Create,
    Roles_Edit,
    Roles_Delete,
    Permissions_Assign
}

