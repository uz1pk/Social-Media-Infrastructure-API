﻿using System.ComponentModel.DataAnnotations;

namespace TweetAPI.Contracts.v1.Requests
{
    public class UserRegistrationRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
