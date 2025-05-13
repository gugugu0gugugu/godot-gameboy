using System;
using Godot;

namespace csharp_branch.src.ui {
    public enum MessageType {
        Info = 1,
        
        Warn = 2,
        Error = 4,
        Debug = 1024,
    }

    public partial class OneMessageBox {
        public string title;
        public string message;
        public MessageType type;

        public OneMessageBox() {
            title = "";
            message = "";
        }
        public delegate void MessageCallback(int btn);

        public OneMessageBox(string title, string message, MessageType type) {
            this.title = title;
            this.message = message;
            this.type = type;
        }

        public OneMessageBox(string title, string message) {
            this.title = title;
            this.message = message;
        }


        static public void Show(string title, string message, string[] btns, Callable? callback) {
            DisplayServer.DialogShow(title, message, btns, callback ?? Callable.From(new Action<int>(Nothing)));
        }

        static private void Nothing(int _) {
        }
    }
}

