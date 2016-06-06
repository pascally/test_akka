﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace test_akka
{
    /// <summary> 
    /// Actor responsible for reading FROM the console.  
    /// Also responsible for calling <see cref="ActorSystem.Shutdown"/>. 
    /// </summary> 
    public class ConsoleReaderActor : UntypedActor 
    { 
        public const string ExitCommand = "exit";
        public const string StartCommand = "start";

        private readonly IActorRef _validationActor;

        public ConsoleReaderActor(IActorRef validationActor)
        {
            _validationActor = validationActor;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }

            GetAndValidateInput();
        }


        #region Internal methods
        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }


        /// <summary>
        /// Reads input from console, validates it, then signals appropriate response
        /// (continue processing, error, success, etc.).
        /// </summary>
        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();
            if (!string.IsNullOrEmpty(message) &&
            String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                // if user typed ExitCommand, shut down the entire actor
                // system (allows the process to exit)
                Context.System.Shutdown();
                return;
            }

            // otherwise, just hand message off to validation actor
            // (by telling its actor ref)
            _validationActor.Tell(message);
        }
        #endregion

    }
}
