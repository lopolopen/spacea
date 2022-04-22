using System;
using System.Collections.Generic;

namespace SpaceA.Model.Dto
{
    public class Error
    {
        public string Message { get; set; }
        public Error(string message)
        {
            Message = message;
        }
    }
}