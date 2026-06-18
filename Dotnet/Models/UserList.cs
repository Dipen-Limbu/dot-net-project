using System;
using System.Collections.Generic;

namespace Dotnet.Models;

public partial class UserList
{
    public short UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string UserPhoto { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string CurrentAddress { get; set; } = null!;

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
