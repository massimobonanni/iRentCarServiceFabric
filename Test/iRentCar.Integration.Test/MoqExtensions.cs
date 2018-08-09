using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace Moq
{
    internal static class MoqExtensions
    {
        public static IReturnsResult<TMock> ReturnsTask<TMock, TResult>(this ISetup<TMock, Task<TResult>> setup, TResult returnValue) where TMock : class
        {
            return setup.Returns(Task.FromResult<TResult>(returnValue));
        }
    }
}
