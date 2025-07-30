using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Galleon.Checkout
{
    public class GenericInspector : Inspector
    {
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public const int MIN_LABEL_WIDTH = 250;
        public const int CHUNK_SIZE      = 100;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // UI
        public Foldout MainFoldout;
        
        // Collections
        private int lastAddedItemIndex = 0;
        
        public bool showFullInheritance = false;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public GenericInspector(object target, string displayName = null) : base(target)
        {
            string DisplayName = displayName.ToSafeString();
            
            // Main Foldout
            MainFoldout       = new Foldout(); this.Add(MainFoldout);
            MainFoldout.text  = "Raw";
            MainFoldout.value = false; // Closed by default
            MainFoldout.RegisterValueChangedCallback(x => { if (x.target == MainFoldout) RefreshUI(); });
            
            
            // Inner toggle
            Toggle  toggle                  = MainFoldout.Q<Toggle>();
            
            // Inner element
            var     defaultElement          = toggle.Children().FirstOrDefault();
            defaultElement.style.flexGrow   = 0;
            defaultElement.style.minWidth   = MIN_LABEL_WIDTH + 6;
            
            
            // Main Button
            var strField                  = new TextField();  toggle.Add(strField);
            strField                      .SetEnabled(false);
            strField                      .isReadOnly = true;
            strField.value                = target.ToSafeString();
            strField.style.height         = 20;
            strField.style.flexGrow       = 1;
            strField.style.flexDirection  = FlexDirection.RowReverse;
            strField.style.unityTextAlign = TextAnchor.MiddleLeft;
            strField.style.paddingLeft    = 10;
        }

        public void RefreshUI()
        {
            // Do nothing if not visible
            if (MainFoldout.value == false)
                return;
            
            // clear
            MainFoldout.contentContainer.Clear();
            
            // refresh button
            var RefreshButton             = new Button(); MainFoldout.contentContainer.Add(RefreshButton);
            RefreshButton.text            = "Refresh";
            RefreshButton.style.maxWidth  = 60;
            RefreshButton.clicked        += RefreshUI;
            
            // content
            var content = CreateGuI();
            MainFoldout.contentContainer.Add(content);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

        protected VisualElement CreateGuI()
        {   
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement Content = new VisualElement();
            
            // InspectorElement.FillDefaultInspector(container        : Content
            //                                      ,serializedObject : serializedObject
            //                                      ,editor           : this);
            
            if (Application.isPlaying)
                FillPropertiesOfObject(Target, Content);
            
            return Content;
        }

        public void FillPropertiesOfObject(object target, VisualElement ui)
        {
            try
            {
                if (target is ICollection collection)
                {
                    var rawViewFoldout = new Foldout(); ui.Add(rawViewFoldout);
                    rawViewFoldout.text  = "Raw View";
                    rawViewFoldout.value = false;
                    FillDefaultUI(target, rawViewFoldout.contentContainer);
                    
                    var itemsFoldout   = new Foldout(); ui.Add(itemsFoldout);
                    itemsFoldout.text  = $"Items ({collection.Count})";
                    itemsFoldout.value = false;
                    FillEnumerableUI(collection, itemsFoldout.contentContainer);
                }
                else
                {
                    FillDefaultUI(target, ui);
                }
            }
            catch (Exception e)
            {
            }
        }
        
        public void FillDefaultUI(object target, VisualElement ui)
        {
            ui.Clear();
            
            ////////////////////
            
            // Get base classes types (in one line)
            List<Type> baseClasses = new List<Type>();
            {
                Type type = target.GetType();
                while (type != typeof(object))
                {
                    baseClasses.Add(type);
                    type = type.BaseType;
                }
            }

            ////////////////////

            foreach (var baseType in baseClasses)
            {
                ////////////////////////////////////////////////////////////////
                
                Label typeLabel                 = new Label(baseType.Name); ui.Add(typeLabel);
                typeLabel.style.paddingTop      = 2;
                typeLabel.style.paddingBottom   = 2;
                typeLabel.style.marginTop       = 5;
                typeLabel.style.marginBottom    = 5;
                typeLabel.style.backgroundColor = new Color(0.0f, 0.3f, 0.3f);
                
                ////////////////////////////////////////////////////////////////
                
                var members = baseType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                      .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);

                foreach (var member in members)
                {
                    try
                    {
                        // Definitions
                        var memberType = member.MemberType  switch { MemberTypes.Field    => ((FieldInfo   )member).FieldType,
                                                                     MemberTypes.Property => ((PropertyInfo)member).PropertyType, };
                        
                        var memberName  = member.Name.ToSafeString();
                        
                        var memberValue = member.MemberType switch { MemberTypes.Field    => ((FieldInfo   )member).GetValue(target),
                                                                     MemberTypes.Property => ((PropertyInfo)member).GetValue(target), };
                        
                        var isPublic    = member.MemberType switch { MemberTypes.Field    => ((FieldInfo   )member).IsPublic,
                                                                     MemberTypes.Property => ((PropertyInfo)member).GetMethod.IsPublic, };
                        
                        var isPrimitive = memberType.IsPrimitive || memberType == typeof(string) || memberType.IsEnum;
                        
                        var isNull      = memberValue is null;
                        
                        var displayText = $"<b>{memberName.ToSafeString()}</b> ({memberType.Name.ToSafeString()})";
                        
                        if (isPrimitive || isNull)
                        {
                            VisualElement field = new VisualElement(); ui.Add(field);
                            
                            // Label
                            TextField text                   = new TextField(); field.Add(text);
                            text.multiline                   = true;
                            text.label                       = displayText ?? "(null)";
                            text.value                       = memberValue.ToSafeString(); 
                            text.labelElement.style.minWidth = MIN_LABEL_WIDTH;
                            
                            // on edit event for text :
                            VisualElement actionsPanel       = new VisualElement(); field.Add(actionsPanel);
                            actionsPanel.style.flexDirection = FlexDirection.Row;
                            actionsPanel.style.flexGrow      = 1;
                            var oldText                      = text.text;
                            text.RegisterValueChangedCallback(evt =>
                                                              {
                                                                  try
                                                                  {
                                                                      var newText = text.text;
                                                                  
                                                                      if (oldText != newText)
                                                                      {                                                                      
                                                                          if (actionsPanel.childCount > 0)
                                                                          {
                                                                              actionsPanel.Clear();
                                                                          }
                                                                      
                                                                          var applyButton       = new Button(); actionsPanel.Add(applyButton);
                                                                          applyButton.text      = "Apply";
                                                                          applyButton.clicked  += () =>
                                                                                                {
                                                                                                    try
                                                                                                    {
                                                                                                       // Get value
                                                                                                       object newValue = Convert.ChangeType(newText, memberType);
                                                                                                       Debug.Log($"new value : {newValue ?? "NULL"}, type : {newValue?.GetType().Name ?? "NULL"}");
                                                                                                       
                                                                                                       // Set value
                                                                                                       if (member.MemberType == MemberTypes.Field   ) 
                                                                                                           ((FieldInfo   )member).SetValue(target, newValue);
                                                                                                       if (member.MemberType == MemberTypes.Property) 
                                                                                                           ((PropertyInfo)member).SetValue(target, newValue);
                                                                                                           
                                                                                                       oldText = newText;
                                                                                                           
                                                                                                       // Remove apply/revert buttons
                                                                                                       actionsPanel.Clear();
                                                                                                    }  
                                                                                                    catch (Exception e)
                                                                                                    {
                                                                                                        Debug.LogException(e);
                                                                                                    }
                                                                                                };
                                                                                                
                                                                            var revertButton      = new Button(); actionsPanel.Add(revertButton);
                                                                            revertButton.text     = "Revert";
                                                                            revertButton.clicked += () =>
                                                                                                  {
                                                                                                      text.value = oldText;
                                                                                                      actionsPanel.Clear();
                                                                                                  };                                            
                                                                      }
                                                                  }
                                                                  catch (Exception ex)
                                                                  {
                                                                      Debug.LogException(ex);
                                                                  }
                                                              });
                            
                            text.SetEnabled(isPublic);
                        }
                        else
                        {
                            // Foldout
                            
                            if (memberValue is null)
                                memberName += " <color=red>(null)</color>";
                            
                            if (memberValue is ICollection c)
                                memberName += $" <color=white>({c.Count})</color>";
                            
                            GenericInspector genericInspector = new GenericInspector(memberValue, displayText); ui.Add(genericInspector);
                        }
                    }
                    catch (Exception e)
                    {
                        TextField errorText = new TextField(); ui.Add(errorText);
                        errorText.value = $"(error : {e.Message})";
                        errorText.label = $"[{member?.Name.ToSafeString()}]";
                    }
                }
                
                ////////////////////////////////////////////////////////////////
                
                // get all methods that return void and have no params
                var methods = baseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                      .Where     (m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                                      .ToArray   ();
                
                if (methods.Length > 0)
                {
                    ButtonFoldout MethodsFoldout               = new ButtonFoldout(); ui.Add(MethodsFoldout);
                    MethodsFoldout.Button.style.maxWidth       = 120;
                    MethodsFoldout.Text                        = $"METHODS ({methods.Count()})";
                    MethodsFoldout.Expanded                    = false; // Closed by default
                    
                    foreach (var method in methods)
                    {
                        try
                        {
                            Button button                = new Button(); MethodsFoldout.Content.Add(button);
                            button.text                  =  method.Name;
                            button.style.unityTextAlign  = TextAnchor.MiddleLeft;
                            button.clicked              += () =>
                                                           {
                                                               if (method.IsStatic)
                                                                 method.Invoke(null, null);
                                                               else
                                                                 method.Invoke(target, null);
                                                           };
                            
                                                        
                            if (!method.IsPublic)
                                button.text += " (private)";
                            
                            if (method.IsStatic)
                                button.text += " (static)";
                        }
                        catch (Exception e)
                        {
                            TextField errorText = new TextField(); MethodsFoldout.Add(errorText);
                            errorText.value = $"(error : {e.Message})";
                            errorText.label = $"[{method?.Name.ToSafeString()}]";
                        }
                    }
                }
                
                ////////////////////////////////////////////////////////////////
                
                
                // get all methods that return void and have no params
                var steps = baseType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                                    .Where     (m => m is PropertyInfo p && p.PropertyType == typeof(Step) 
                                                  || m is MethodInfo mi && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(Step) && !mi.Name.StartsWith("get_"))
                                    .ToArray   ();
                
                if (steps.Length > 0)
                {
                    ButtonFoldout StepsFoldout               = new ButtonFoldout(); ui.Add(StepsFoldout);
                    StepsFoldout.Button.style.maxWidth       = 120;
                    StepsFoldout.Text                        = $"STEPS ({steps.Count()})";
                    StepsFoldout.Expanded                    = false; // Closed by default
                    
                    foreach (var step in steps)
                    {
                        try
                        {
                            Button button                = new Button(); StepsFoldout.Content.Add(button);
                            button.text                  =  step.Name;
                            button.style.unityTextAlign  = TextAnchor.MiddleLeft;
                            button.clicked              += () =>
                                                           {
                                                               if (step is MethodInfo m)
                                                               {
                                                                   Step s;
                                                                   
                                                                   if (m.IsStatic)
                                                                     s = m.Invoke(null, null) as Step;
                                                                   else
                                                                     s = m.Invoke(target, null) as Step;
                                                                   
                                                                   s.Execute();
                                                               }
                                                               if (step is PropertyInfo p)
                                                               {
                                                                   Step s;
                                                                   if (p.GetMethod?.IsStatic ?? false)
                                                                       s = p.GetValue(null) as Step;
                                                                   else
                                                                       s = p.GetValue(target) as Step;
                                                                       
                                                                   s.Execute();
                                                               }
                                                           };
                            
                            bool isPublic = (step is MethodInfo methodInfo && methodInfo.IsPublic) || (step is PropertyInfo propertyInfo && propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic);
                            if (!isPublic)
                                button.text += " (private)";
                            bool isStatic =  (step is MethodInfo methodInfoCheck && methodInfoCheck.IsStatic || step is PropertyInfo propertyInfoCheck && propertyInfoCheck.GetMethod?.IsStatic == true);
                            if (isStatic)
                                button.text += " (static)";
                        }
                        catch (Exception e)
                        {
                            TextField errorText = new TextField(); StepsFoldout.Add(errorText);
                            errorText.value = $"(error : {e.Message})";
                            errorText.label = $"method = [{step?.Name.ToSafeString()}]";
                        }
                    }
                }

                
                ////////////////////////////////////////////////////////////////
                
                if (baseType            == baseClasses.First()
                &&  showFullInheritance == false
                &&  baseClasses.Count   > 1)
                {
                    Button showFullInheritanceButton                 = new Button(); ui.Add(showFullInheritanceButton);
                    showFullInheritanceButton.text                   = " + ";
                    showFullInheritanceButton.clicked               += () => { showFullInheritance = true; FillDefaultUI(target, ui); };
                    showFullInheritanceButton.style.backgroundColor  = new Color(0.0f, 0.3f, 0.3f);

                    break;
                }
                
                ////////////////////////////////////////////////////////////////

            }
        }
        
        public void FillEnumerableUI(ICollection collection, VisualElement ui)
        {
            // Child Items
            
            int i = -1;
            
            foreach (var child in collection)
            {
                try
                {
                    i++;
                    
                    // if already created this item - continue
                    if (i < lastAddedItemIndex)
                        continue;
                    
                    // Create and add a Child inspector
                    var inspector = new GenericInspector(child, $"[{i,-3} - {child?.ToSafeString()}]"); 
                    ui.Add(inspector);
                    
                    // if we have a full chunk, just add a "load more" button, and stop here
                    if (lastAddedItemIndex > 0 && (lastAddedItemIndex+1) % CHUNK_SIZE == 0)
                    {
                        lastAddedItemIndex += 1;
                        
                        Button loadMoreButton   = new Button(); ui.Add(loadMoreButton);
                        loadMoreButton.text     =  $"Load {CHUNK_SIZE} More";
                        loadMoreButton.clicked += () =>
                                                  {
                                                      loadMoreButton.SetEnabled(false);
                                                      FillEnumerableUI(collection, ui);
                                                  };
                        
                        return;
                    }
                    
                }
                catch (Exception ex)
                {
                    Label errorLabel                 = new Label($"error"); ui.Add(errorLabel);
                    errorLabel.style.backgroundColor = new Color(0.5f, 0, 0);
                    errorLabel.style.marginBottom    = 2;
                    errorLabel.style.marginTop       = 2;
                    
                    if (child != null)
                        errorLabel.text += $" - {child.ToSafeString()}";
                }
                
                lastAddedItemIndex++;
            }
        }
    }
}
