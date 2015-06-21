﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LiteGuard;

namespace LeagueRecorder.Shared.Abstractions.Results
{
    public class Result
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// This constructor is internal, so the user has to use the factory methods in the <see cref="Result"/> class.
        /// </summary>
        internal Result()
        {
            this.AdditionalData = new Dictionary<string, object>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether <see cref="State"/> is <see cref="ResultState.Success"/>.
        /// </summary>
        public bool IsSuccess
        {
            get { return this.State == ResultState.Success; }
        }
        /// <summary>
        /// Gets a value indicating whether <see cref="State"/> is <see cref="ResultState.Error"/>.
        /// </summary>
        public bool IsError
        {
            get { return this.State == ResultState.Error; }
        }
        /// <summary>
        /// Gets the more detailed state.
        /// For more simple cases you can use the properties <see cref="IsSuccess"/> and <see cref="IsError"/>.
        /// </summary>
        public ResultState State { get; internal set; }
        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; internal set; }
        /// <summary>
        /// Gets the additional data.
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; internal set; } 
        #endregion

        #region Factory Methods
        /// <summary>
        /// Creates a new <see cref="Result"/> with <see cref="Result.State"/> = <see cref="ResultState.Success"/>.
        /// </summary>
        [DebuggerStepThrough]
        public static Result AsSuccess()
        {
            return new Result
            {
                State = ResultState.Success
            };
        }
        /// <summary>
        /// Creates a new <see cref="Result"/> with the specified <paramref name="message"/> and <see cref="Result.State"/> = <see cref="ResultState.Error"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        [DebuggerStepThrough]
        public static Result AsError(string message)
        {
            Guard.AgainstNullArgument("message", message);

            return new Result
            {
                State = ResultState.Error,
                Message = message
            };
        }
        /// <summary>
        /// Creates a new <see cref="Result"/> with the specified <see cref="Exception.Message"/> and <see cref="Result.State"/> = <see cref="ResultState.Error"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        [DebuggerStepThrough]
        public static Result FromException(Exception exception)
        {
            Guard.AgainstNullArgument("exception", exception);

            return new Result
            {
                State = ResultState.Error,
                Message = exception.Message
            };
        }
        /// <summary>
        /// Creates a new <see cref="Result{T}"/> with the specified <paramref name="data"/> and <see cref="Result{T}.State"/> = <see cref="ResultState.Success"/>.
        /// </summary>
        /// <typeparam name="T">The type of data the result can contain.</typeparam>
        /// <param name="data">The data.</param>
        [DebuggerStepThrough]
        public static Result<T> AsSuccess<T>(T data)
        {
            return new Result<T>
            {
                State = ResultState.Success,
                Data = data
            };
        }
        /// <summary>
        /// Creates a new <see cref="Result{T}"/> containing <see cref="Result{T}.State"/> = <see cref="ResultState.Success"/> if the specified <paramref name="action"/> could be executed without a <see cref="Exception"/>.
        /// If a <see cref="Exception"/> occured the <see cref="Result{T}.State"/> will equal to <see cref="ResultState.Error"/>.
        /// </summary>
        /// <typeparam name="T">The type of data the result can contain.</typeparam>
        /// <param name="action">The action.</param>
        [DebuggerStepThrough]
        public static Result<T> Create<T>(Func<T> action)
        {
            Guard.AgainstNullArgument("action", action);

            try
            {
                return Result.AsSuccess(action());
            }
            catch (ResultException resultException)
            {
                var result = Result.FromException(resultException);
                result.AdditionalData = resultException.AdditionalData;

                return result;
            }
            catch (Exception exception)
            {
                return Result.FromException(exception);
            }
        }
        /// <summary>
        /// Creates a new <see cref="Result{T}"/> containing <see cref="Result{T}.State"/> = <see cref="ResultState.Success"/> if the specified <paramref name="action"/> could be executed without a <see cref="Exception"/>.
        /// If a <see cref="Exception"/> occured the <see cref="Result{T}.State"/> will equal to <see cref="ResultState.Error"/>.
        /// </summary>
        /// <typeparam name="T">The type of data the result can contain.</typeparam>
        /// <param name="action">The action.</param>
        [DebuggerStepThrough]
        public static async Task<Result<T>> CreateAsync<T>(Func<Task<T>> action)
        {
            Guard.AgainstNullArgument("action", action);

            try
            {
                return Result.AsSuccess(await action().ConfigureAwait(false));
            }
            catch (ResultException resultException)
            {
                var result = Result.FromException(resultException);
                result.AdditionalData = resultException.AdditionalData;

                return result;
            }
            catch (Exception exception)
            {
                return Result.FromException(exception);
            }
        }
        /// <summary>
        /// Creates a new <see cref="Result"/> containing <see cref="Result{T}.State"/> = <see cref="ResultState.Success"/> if the specified <paramref name="action"/> could be executed without a <see cref="Exception"/>.
        /// If a <see cref="Exception"/> occured the <see cref="Result{T}.State"/> will equal to <see cref="ResultState.Error"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerStepThrough]
        public static Result Create(Action action)
        {
            Guard.AgainstNullArgument("action", action);
            
            try
            {
                action();
                return Result.AsSuccess();
            }
            catch (ResultException resultException)
            {
                var result = Result.FromException(resultException);
                result.AdditionalData = resultException.AdditionalData;

                return result;
            }
            catch (Exception exception)
            {
                return Result.FromException(exception);
            }
        }
        /// <summary>
        /// Creates a new <see cref="Result"/> containing <see cref="Result{T}.State"/> = <see cref="ResultState.Success"/> if the specified <paramref name="action"/> could be executed without a <see cref="Exception"/>.
        /// If a <see cref="Exception"/> occured the <see cref="Result{T}.State"/> will equal to <see cref="ResultState.Error"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        [DebuggerStepThrough]
        public static async Task<Result> CreateAsync(Func<Task> action)
        {
            Guard.AgainstNullArgument("action", action);

            try
            {
                await action().ConfigureAwait(false);
                return Result.AsSuccess();
            }
            catch (ResultException resultException)
            {
                var result = Result.FromException(resultException);
                result.AdditionalData = resultException.AdditionalData;

                return result;
            }
            catch (Exception exception)
            {
                return Result.FromException(exception);
            }
        }
        #endregion
    }
}