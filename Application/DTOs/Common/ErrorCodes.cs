﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Common
{
    public static class ErrorCodes
    {
        // 🔹 Validation
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string INVALID_INPUT = "INVALID_INPUT";

        // 🔹 Auth
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
        public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
        public const string INVALID_TOKEN = "INVALID_TOKEN";

        // 🔹 User
        public const string USER_NOT_FOUND = "USER_NOT_FOUND";
        public const string USER_ALREADY_EXISTS = "USER_ALREADY_EXISTS";

        // 🔹 Data & Resource
        public const string NOT_FOUND = "NOT_FOUND";
        public const string ALREADY_EXISTS = "ALREADY_EXISTS";
        public const string CONFLICT = "CONFLICT";
        public const string DB_ERROR = "DB_ERROR";

        // 🔹 Server
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string BAD_REQUEST = "BAD_REQUEST";
        public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
        public const string TIMEOUT = "TIMEOUT";

        // 🔹 Business logic
        public const string INSUFFICIENT_BALANCE = "INSUFFICIENT_BALANCE";
        public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    }
}
