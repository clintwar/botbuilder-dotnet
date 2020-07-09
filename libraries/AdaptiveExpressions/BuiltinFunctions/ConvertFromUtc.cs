﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Convert a timestamp from Universal Time Coordinated (UTC) to a target time zone.
    /// </summary>
    public class ConvertFromUtc : ExpressionEvaluator
    {
        public ConvertFromUtc()
            : base(ExpressionType.ConvertFromUtc, Evaluator, ReturnType.String, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
            var format = FunctionUtils.DefaultDateTimeFormat;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 4);
            }

            if (error == null)
            {
                if (args[1] is string targetTimeZone)
                {
                    (value, error) = EvalConvertFromUTC(args[0], targetTimeZone, format, locale);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a destination time zone string, an optional output format string and an optional locale string.";
                }
            }

            return (value, error);
        }

        private static (string, string) EvalConvertFromUTC(object utcTimestamp, string timezone, string format, CultureInfo locale)
        {
            string error = null;
            string result = null;
            var utcDt = DateTime.UtcNow;
            object parsed = null;
            object convertedTimeZone = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(utcTimestamp);
            if (error == null)
            {
                utcDt = ((DateTime)parsed).ToUniversalTime();
            }

            if (error == null)
            {
                (convertedTimeZone, error) = FunctionUtils.ConvertTimeZoneFormat(timezone);

                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(convertedDateTime, format, locale);
                }
            }
             
            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 4, ReturnType.String);
        }
    }
}