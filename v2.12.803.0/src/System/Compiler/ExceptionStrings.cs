namespace System.Compiler
{
    using System;
    using System.Resources;

    internal sealed class ExceptionStrings
    {
        private static readonly WeakReference resMgr = new WeakReference(null);

        private ExceptionStrings()
        {
        }

        internal static string AssemblyReferenceNotResolved =>
            ResourceManager.GetString("AssemblyReferenceNotResolved", null);

        internal static string BadBlobHeapIndex =>
            ResourceManager.GetString("BadBlobHeapIndex", null);

        internal static string BadCLIHeader =>
            ResourceManager.GetString("BadCLIHeader", null);

        internal static string BadCOFFHeaderSignature =>
            ResourceManager.GetString("BadCOFFHeaderSignature", null);

        internal static string BadConstantParentIndex =>
            ResourceManager.GetString("BadConstantParentIndex", null);

        internal static string BadCustomAttributeTypeEncodedToken =>
            ResourceManager.GetString("BadCustomAttributeTypeEncodedToken", null);

        internal static string BaddCalliSignature =>
            ResourceManager.GetString("BaddCalliSignature", null);

        internal static string BadExceptionHandlerType =>
            ResourceManager.GetString("BadExceptionHandlerType", null);

        internal static string BadGuidHeapIndex =>
            ResourceManager.GetString("BadGuidHeapIndex", null);

        internal static string BadMagicNumber =>
            ResourceManager.GetString("BadMagicNumber", null);

        internal static string BadMemberToken =>
            ResourceManager.GetString("BadMemberToken", null);

        internal static string BadMetadataHeaderSignature =>
            ResourceManager.GetString("BadMetadataHeaderSignature", null);

        internal static string BadMetadataInExportTypeTableNoSuchAssemblyReference =>
            ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchAssemblyReference", null);

        internal static string BadMetadataInExportTypeTableNoSuchParentType =>
            ResourceManager.GetString("BadMetadataInExportTypeTableNoSuchParentType", null);

        internal static string BadMethodHeaderSection =>
            ResourceManager.GetString("BadMethodHeaderSection", null);

        internal static string BadMethodTypeParameterInPosition =>
            ResourceManager.GetString("BadMethodTypeParameterInPosition", null);

        internal static string BadPEHeaderMagicNumber =>
            ResourceManager.GetString("BadPEHeaderMagicNumber", null);

        internal static string BadSecurityPermissionSetBlob =>
            ResourceManager.GetString("BadSecurityPermissionSetBlob", null);

        internal static string BadSerializedTypeName =>
            ResourceManager.GetString("BadSerializedTypeName", null);

        internal static string BadStringHeapIndex =>
            ResourceManager.GetString("BadStringHeapIndex", null);

        internal static string BadTargetPlatformLocation =>
            ResourceManager.GetString("BadTargetPlatformLocation", null);

        internal static string BadTypeDefOrRef =>
            ResourceManager.GetString("BadTypeDefOrRef", null);

        internal static string BadTypeParameterInPositionForType =>
            ResourceManager.GetString("BadTypeParameterInPositionForType", null);

        internal static string BadUserStringHeapIndex =>
            ResourceManager.GetString("BadUserStringHeapIndex", null);

        internal static string CannotLoadTypeExtension =>
            ResourceManager.GetString("CannotLoadTypeExtension", null);

        internal static string CollectionIsReadOnly =>
            ResourceManager.GetString("CollectionIsReadOnly", null);

        internal static string CouldNotFindExportedNestedTypeInType =>
            ResourceManager.GetString("CouldNotFindExportedNestedTypeInType", null);

        internal static string CouldNotFindExportedTypeInAssembly =>
            ResourceManager.GetString("CouldNotFindExportedTypeInAssembly", null);

        internal static string CouldNotFindExportedTypeInModule =>
            ResourceManager.GetString("CouldNotFindExportedTypeInModule", null);

        internal static string CouldNotFindReferencedModule =>
            ResourceManager.GetString("CouldNotFindReferencedModule", null);

        internal static string CouldNotResolveMemberReference =>
            ResourceManager.GetString("CouldNotResolveMemberReference", null);

        internal static string CouldNotResolveType =>
            ResourceManager.GetString("CouldNotResolveType", null);

        internal static string CouldNotResolveTypeReference =>
            ResourceManager.GetString("CouldNotResolveTypeReference", null);

        internal static string CreateFileMappingReturnedErrorCode =>
            ResourceManager.GetString("CreateFileMappingReturnedErrorCode", null);

        internal static string ENCLogTableEncountered =>
            ResourceManager.GetString("ENCLogTableEncountered", null);

        internal static string ENCMapTableEncountered =>
            ResourceManager.GetString("ENCMapTableEncountered", null);

        internal static string FileTooBig =>
            ResourceManager.GetString("FileTooBig", null);

        internal static string GetReaderForFileReturnedUnexpectedHResult =>
            ResourceManager.GetString("GetReaderForFileReturnedUnexpectedHResult", null);

        internal static string InternalCompilerError =>
            ResourceManager.GetString("InternalCompilerError", null);

        internal static string InvalidAssemblyStrongName =>
            ResourceManager.GetString("InvalidAssemblyStrongName", null);

        internal static string InvalidBaseClass =>
            ResourceManager.GetString("InvalidBaseClass", null);

        internal static string InvalidFatMethodHeader =>
            ResourceManager.GetString("InvalidFatMethodHeader", null);

        internal static string InvalidLocalSignature =>
            ResourceManager.GetString("InvalidLocalSignature", null);

        internal static string InvalidModuleTable =>
            ResourceManager.GetString("InvalidModuleTable", null);

        internal static string InvalidTypeTableIndex =>
            ResourceManager.GetString("InvalidTypeTableIndex", null);

        internal static string KeyNeedsToBeGreaterThanZero =>
            ResourceManager.GetString("KeyNeedsToBeGreaterThanZero", null);

        internal static string MalformedSignature =>
            ResourceManager.GetString("MalformedSignature", null);

        internal static string MapViewOfFileReturnedErrorCode =>
            ResourceManager.GetString("MapViewOfFileReturnedErrorCode", null);

        internal static string ModuleError =>
            ResourceManager.GetString("ModuleError", null);

        internal static string ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary =>
            ResourceManager.GetString("ModuleOrAssemblyDependsOnMoreRecentVersionOfCoreLibrary", null);

        internal static string NoMetadataStream =>
            ResourceManager.GetString("NoMetadataStream", null);

        internal static string PdbAssociatedWithFileIsOutOfDate =>
            ResourceManager.GetString("PdbAssociatedWithFileIsOutOfDate", null);

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                System.Resources.ResourceManager target = resMgr.Target as System.Resources.ResourceManager;
                if (target == null)
                {
                    target = new System.Resources.ResourceManager("System.Compiler.ExceptionStrings", typeof(ExceptionStrings).Assembly);
                    resMgr.Target = target;
                }
                return target;
            }
        }

        internal static string SecurityAttributeTypeDoesNotHaveADefaultConstructor =>
            ResourceManager.GetString("SecurityAttributeTypeDoesNotHaveADefaultConstructor", null);

        internal static string TooManyMethodHeaderSections =>
            ResourceManager.GetString("TooManyMethodHeaderSections", null);

        internal static string UnexpectedTypeInCustomAttribute =>
            ResourceManager.GetString("UnexpectedTypeInCustomAttribute", null);

        internal static string UnknownConstantType =>
            ResourceManager.GetString("UnknownConstantType", null);

        internal static string UnknownOpCode =>
            ResourceManager.GetString("UnknownOpCode", null);

        internal static string UnknownOpCodeEncountered =>
            ResourceManager.GetString("UnknownOpCodeEncountered", null);

        internal static string UnknownVirtualAddress =>
            ResourceManager.GetString("UnknownVirtualAddress", null);

        internal static string UnresolvedAssemblyReferenceNotAllowed =>
            ResourceManager.GetString("UnresolvedAssemblyReferenceNotAllowed", null);

        internal static string UnresolvedModuleReferenceNotAllowed =>
            ResourceManager.GetString("UnresolvedModuleReferenceNotAllowed", null);

        internal static string UnsupportedTableEncountered =>
            ResourceManager.GetString("UnsupportedTableEncountered", null);
    }
}

