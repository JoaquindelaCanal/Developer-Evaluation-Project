﻿using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}