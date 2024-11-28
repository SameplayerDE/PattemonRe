using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HxCameraEditor.UserInterface
{
    public class Button : UserInterfaceNodeContainer
    {
        public Action? Action { get; private set; }
        public float PaddingLeft { get; private set; } = 25;
        public float PaddingTop { get; private set; } = 5;
        public float PaddingRight { get; private set; } = 25;
        public float PaddingBottom { get; private set; } = 5;
        public UserInterfaceNode? ToolTip { get; private set; }
        public bool IsDisabled { get; set; } = false;
        public Binding<bool>? IsDisabledBinding;

        public Button(params UserInterfaceNode[] nodes) : base(UserInterfaceNodeType.Button)
        {
            IsClickable = true;
            Children.AddRange(nodes);
        }
        
        public Button(IEnumerable<UserInterfaceNode> nodes) : base(UserInterfaceNodeType.Button)
        {
            IsClickable = true;
            Children.AddRange(nodes);
        }

        public Button SetIsDisabledBinding(Binding<bool> binding)
        {
            IsDisabledBinding = binding;
            IsDisabledBinding!.ValueChanged += OnIsDisabledChanged;
            UpdateIsDisabled();
            return this;
        }
        
        private void OnIsDisabledChanged(bool value)
        {
            IsDisabled = value;
        }

        private void UpdateIsDisabled()
        {
            if (IsDisabledBinding != null)
            {
                IsDisabled = IsDisabledBinding.Value;
            }
        }
        
       //public Button(string image, string text) : this(new HStack(new Image(image), new Label(text))
       //    .SetAlignment(Alignment.Center).SetSpacing(10).SetPadding(10)) {}

       public Button SetIsDisabled()
       {
           IsDisabled = true;
           return this;
       }
       
        public Button SetToolTip(UserInterfaceNode toolTip)
        {
            ToolTip = toolTip;
            return this;
        }
        
        public Button SetPadding(float value)
        {
            SetPaddingLeft(value);
            SetPaddingTop(value);
            SetPaddingRight(value);
            SetPaddingBottom(value);
            return this;
        }
        
        public Button SetPaddingLeft(float value)
        {
            PaddingLeft = value;
            return this;
        }

        public Button SetPaddingTop(float value)
        {
            PaddingTop = value;
            return this;
        }

        public Button SetPaddingRight(float value)
        {
            PaddingRight = value;
            return this;
        }

        public Button SetPaddingBottom(float value)
        {
            PaddingBottom = value;
            return this;
        }

        public Button OnClick(Action action)
        {
            Action = action;
            return this;
        }

        public void Invoke()
        {
            Action?.Invoke();
        }
    }
}