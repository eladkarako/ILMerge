namespace ILMergeGui.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Resources
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        internal static string AssembliesMerged =>
            ResourceManager.GetString("AssembliesMerged", resourceCulture);

        internal static Bitmap btn_donateCC_LG_EN =>
            ((Bitmap) ResourceManager.GetObject("btn_donateCC_LG_EN", resourceCulture));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => 
                resourceCulture;
            set
            {
                resourceCulture = value;
            }
        }

        internal static string Done =>
            ResourceManager.GetString("Done", resourceCulture);

        internal static string Error_CantMerge =>
            ResourceManager.GetString("Error_CantMerge", resourceCulture);

        internal static string Error_Framework =>
            ResourceManager.GetString("Error_Framework", resourceCulture);

        internal static string Error_KeyFileNotExists =>
            ResourceManager.GetString("Error_KeyFileNotExists", resourceCulture);

        internal static string Error_MergeException =>
            ResourceManager.GetString("Error_MergeException", resourceCulture);

        internal static string Error_NoOutputPath =>
            ResourceManager.GetString("Error_NoOutputPath", resourceCulture);

        internal static string Error_OutputConflict =>
            ResourceManager.GetString("Error_OutputConflict", resourceCulture);

        internal static string Error_OutputPathInUse =>
            ResourceManager.GetString("Error_OutputPathInUse", resourceCulture);

        internal static string Error_Term =>
            ResourceManager.GetString("Error_Term", resourceCulture);

        internal static Bitmap IconAdd =>
            ((Bitmap) ResourceManager.GetObject("IconAdd", resourceCulture));

        internal static Bitmap IconDropHere =>
            ((Bitmap) ResourceManager.GetObject("IconDropHere", resourceCulture));

        internal static Bitmap IconFolder =>
            ((Bitmap) ResourceManager.GetObject("IconFolder", resourceCulture));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                {
                    resourceMan = new System.Resources.ResourceManager("ILMergeGui.Properties.Resources", typeof(Resources).Assembly);
                }
                return resourceMan;
            }
        }
    }
}

