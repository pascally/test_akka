using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.IO;

namespace test_akka
{
        /// <summary>
        /// Actor that validates user input and signals result to others.
        /// </summary>
        public class FileValidatorActor : UntypedActor
        {
            private readonly IActorRef _consoleWriterActor;
            //private readonly IActorRef _tailCoordinatorActor;

            public FileValidatorActor(IActorRef consoleWriterActor)
            {
                _consoleWriterActor = consoleWriterActor;
                //_tailCoordinatorActor = tailCoordinatorActor;
            }

            protected override void OnReceive(object message)
            {
                var msg = message as string;
                if (string.IsNullOrEmpty(msg))
                {
                    // signal that the user needs to supply an input
                    _consoleWriterActor.Tell(new Message.NullInputError("Input was blank. Please try again.\n"));

                    // tell sender to continue doing its thing (whatever that may be,
                    // this actor doesn't care)
                    Sender.Tell(new Message.ContinueProcessing());
                }
                else
                {
                    var valid = IsFileUri(msg);
                    if (valid)
                    {
                        // signal successful input
                        _consoleWriterActor.Tell(new Message.InputSuccess(
                            string.Format("Starting processing for {0}", msg)));

                    // start coordinator
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor")
                        .Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));

                    //_tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                    }
                    else
                    {
                        // signal that input was bad
                        _consoleWriterActor.Tell(new Message.ValidationError(
                            string.Format("{0} is not an existing URI on disk.", msg)));

                        // tell sender to continue doing its thing (whatever that
                        // may be, this actor doesn't care)
                        Sender.Tell(new Message.ContinueProcessing());
                    }
                }
            }

            /// <summary>
            /// Checks if file exists at path provided by user.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            private static bool IsFileUri(string path)
            {
                return File.Exists(path);
            }
        }
}
