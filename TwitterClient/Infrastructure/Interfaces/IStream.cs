using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Enum;
using TwitterClient.Infrastructure.Events.EventArguments;

namespace TwitterClient.Infrastructure
{
    public interface IStream
    {
        void StartStream();

        Task StartStreamAsync();


        /// <summary>
        /// Resume a stopped Stream
        /// </summary>
        void ResumeStream();

        /// <summary>
        /// Pause a running Stream
        /// </summary>
        void PauseStream();

        /// <summary>
        /// Stop a running or paused stream
        /// </summary>
        void StopStream();
    }
}
