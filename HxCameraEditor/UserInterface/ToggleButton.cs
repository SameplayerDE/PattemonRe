using System;

namespace HxCameraEditor.UserInterface;

public class RadioButton : UserInterfaceNode
{
    public bool Checked = false;public Action<bool>? Action { get; private set; }
    public float PaddingLeft { get; private set; } = 25;
    public float PaddingTop { get; private set; } = 5;
    public float PaddingRight { get; private set; } = 25;
    public float PaddingBottom { get; private set; } = 5;
    public UserInterfaceNode? ToolTip { get; private set; }
    public Binding<bool>? IsCheckedBinding { get; private set; }
            
    public RadioButton() : base(UserInterfaceNodeType.RadioButton)
    {
        IsClickable = true;
    }
            
    public RadioButton SetIsCheckedBinding(Binding<bool> binding)
    {
        IsCheckedBinding = binding;
        IsCheckedBinding!.ValueChanged += OnCheckedChanged;
        UpdateIsDisabled();
        return this;
    }
        
    private void OnCheckedChanged(bool value)
    {
        Checked = value;
    }

    private void UpdateIsDisabled()
    {
        if (IsCheckedBinding != null)
        {
            Checked = IsCheckedBinding.Value;
        }
    }
    
    public RadioButton SetPadding(float value)
    {
        SetPaddingLeft(value);
        SetPaddingTop(value);
        SetPaddingRight(value);
        SetPaddingBottom(value);
        return this;
    }
            
    public RadioButton SetPaddingLeft(float value)
    {
        PaddingLeft = value;
        return this;
    }
    
    public RadioButton SetPaddingTop(float value)
    {
        PaddingTop = value;
        return this;
    }
    
    public RadioButton SetPaddingRight(float value)
    {
        PaddingRight = value;
        return this;
    }
    
    public RadioButton SetPaddingBottom(float value)
    {
        PaddingBottom = value;
        return this;
    }
    
    public RadioButton OnClick(Action<bool> action)
    {
        Action = action;
        return this;
    }
    
    public void Invoke()
    {
        Checked = !Checked;
        Action?.Invoke(Checked);
    }
}

public class ToggleButton : UserInterfaceNode
{
    public bool Checked = false;public Action? Action { get; private set; }
    public float PaddingLeft { get; private set; } = 25;
    public float PaddingTop { get; private set; } = 5;
    public float PaddingRight { get; private set; } = 25;
    public float PaddingBottom { get; private set; } = 5;
    public UserInterfaceNode? ToolTip { get; private set; }
        
    public ToggleButton() : base(UserInterfaceNodeType.ToggleButton)
    {
        IsClickable = true;
    }
        
        
    public ToggleButton SetPadding(float value)
    {
        SetPaddingLeft(value);
        SetPaddingTop(value);
        SetPaddingRight(value);
        SetPaddingBottom(value);
        return this;
    }
        
    public ToggleButton SetPaddingLeft(float value)
    {
        PaddingLeft = value;
        return this;
    }

    public ToggleButton SetPaddingTop(float value)
    {
        PaddingTop = value;
        return this;
    }

    public ToggleButton SetPaddingRight(float value)
    {
        PaddingRight = value;
        return this;
    }

    public ToggleButton SetPaddingBottom(float value)
    {
        PaddingBottom = value;
        return this;
    }

    public ToggleButton OnClick(Action action)
    {
        Action = action;
        return this;
    }

    public void Invoke()
    {
        Checked = !Checked;
        Action?.Invoke();
    }
}