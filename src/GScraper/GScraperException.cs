using System;

namespace GScraper
{
    /// <summary>
    /// The exception that is thrown during the scraping process.
    /// </summary>
    public class GScraperException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GScraperException"/> class.
        /// </summary>
        public GScraperException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message.
        /// </summary>
        public GScraperException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public GScraperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}