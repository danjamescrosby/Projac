﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Projac.Testing
{
    /// <summary>
    /// Represent a <see cref="Projection{TConnection}"/> test specification.
    /// </summary>
    /// <typeparam name="TConnection">The type of the connection.</typeparam>
    public class ProjectionTestSpecification<TConnection>
    {
        private readonly ProjectionHandlerResolver<TConnection> _resolver;
        private readonly object[] _messages;
        private readonly Func<TConnection, CancellationToken, Task<VerificationResult>> _verification;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionTestSpecification{TConnection}"/> class.
        /// </summary>
        /// <param name="resolver">The projection handler resolver.</param>
        /// <param name="messages">The messages to project.</param>
        /// <param name="verification">The verification method.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when
        /// <paramref name="resolver"/>
        /// or
        /// <paramref name="messages"/>
        /// or
        /// <paramref name="verification"/> is null.
        /// </exception>
        public ProjectionTestSpecification(ProjectionHandlerResolver<TConnection> resolver, object[] messages, Func<TConnection, CancellationToken, Task<VerificationResult>> verification)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (verification == null) throw new ArgumentNullException(nameof(verification));
            _resolver = resolver;
            _messages = messages;
            _verification = verification;
        }

        /// <summary>
        /// Gets the projection handler resolver.
        /// </summary>
        /// <value>
        /// The projection handler resolver.
        /// </value>
        public ProjectionHandlerResolver<TConnection> Resolver
        {
            get { return _resolver; }
        }

        /// <summary>
        /// Gets the messages to project.
        /// </summary>
        /// <value>
        /// The messages to project.
        /// </value>
        public object[] Messages
        {
            get { return _messages; }
        }

        /// <summary>
        /// Gets the verification method.
        /// </summary>
        /// <value>
        /// The verification method.
        /// </value>
        public Func<TConnection, CancellationToken, Task<VerificationResult>> Verification
        {
            get { return _verification; }
        }
    }
}
