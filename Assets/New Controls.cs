//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/New Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @NewControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @NewControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""New Controls"",
    ""maps"": [
        {
            ""name"": ""Reader"",
            ""id"": ""9b0dda63-8e5e-40c9-aa06-4ee02bc948a5"",
            ""actions"": [
                {
                    ""name"": ""Keyboard"",
                    ""type"": ""Button"",
                    ""id"": ""f1e194f8-4616-4327-864e-c22b76b8da7c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""99b5b677-f6c6-4f5b-8207-d2c96c3874dc"",
                    ""path"": ""<Keyboard>/anyKey"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Reader
        m_Reader = asset.FindActionMap("Reader", throwIfNotFound: true);
        m_Reader_Keyboard = m_Reader.FindAction("Keyboard", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Reader
    private readonly InputActionMap m_Reader;
    private IReaderActions m_ReaderActionsCallbackInterface;
    private readonly InputAction m_Reader_Keyboard;
    public struct ReaderActions
    {
        private @NewControls m_Wrapper;
        public ReaderActions(@NewControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Keyboard => m_Wrapper.m_Reader_Keyboard;
        public InputActionMap Get() { return m_Wrapper.m_Reader; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ReaderActions set) { return set.Get(); }
        public void SetCallbacks(IReaderActions instance)
        {
            if (m_Wrapper.m_ReaderActionsCallbackInterface != null)
            {
                @Keyboard.started -= m_Wrapper.m_ReaderActionsCallbackInterface.OnKeyboard;
                @Keyboard.performed -= m_Wrapper.m_ReaderActionsCallbackInterface.OnKeyboard;
                @Keyboard.canceled -= m_Wrapper.m_ReaderActionsCallbackInterface.OnKeyboard;
            }
            m_Wrapper.m_ReaderActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Keyboard.started += instance.OnKeyboard;
                @Keyboard.performed += instance.OnKeyboard;
                @Keyboard.canceled += instance.OnKeyboard;
            }
        }
    }
    public ReaderActions @Reader => new ReaderActions(this);
    public interface IReaderActions
    {
        void OnKeyboard(InputAction.CallbackContext context);
    }
}
