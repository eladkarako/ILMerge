namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    internal interface IMetaDataImport
    {
        [PreserveSig]
        void CloseEnum(uint hEnum);
        uint CountEnum(uint hEnum);
        void ResetEnum(uint hEnum, uint ulPos);
        uint EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeDefs, uint cMax);
        uint EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rImpls, uint cMax);
        uint EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeRefs, uint cMax);
        uint FindTypeDefByName(string szTypeDef, uint tkEnclosingClass);
        Guid GetScopeProps(StringBuilder szName, uint cchName, out uint pchName);
        uint GetModuleFromScope();
        uint GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags);
        uint GetInterfaceImplProps(uint iiImpl, out uint pClass);
        uint GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName);
        uint ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope);
        uint EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMembers, uint cMax);
        uint EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMembers, uint cMax);
        unsafe uint EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax);
        uint EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethods, uint cMax);
        unsafe uint EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax);
        uint EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rFields, uint cMax);
        uint EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rParams, uint cMax);
        uint EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMemberRefs, uint cMax);
        uint EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodBody, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodDecl, uint cMax);
        uint EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rPermission, uint cMax);
        uint FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
        uint FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob);
        uint GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA);
        unsafe uint GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob);
        unsafe uint EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax);
        unsafe uint EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax);
        uint GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=11)] uint[] rmdOtherMethod, uint cMax);
        uint EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rEventProp, uint cMax);
        uint GetMethodSemantics(uint mb, uint tkEventProp);
        uint GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset);
        unsafe uint GetFieldMarshal(uint tk, out byte* ppvNativeType);
        uint GetRVA(uint tk, out uint pulCodeRVA);
        unsafe uint GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission);
        unsafe uint GetSigFromToken(uint mdSig, out byte* ppvSig);
        uint GetModuleRefProps(uint mur, StringBuilder szName, uint cchName);
        uint EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rModuleRefs, uint cmax);
        unsafe uint GetTypeSpecFromToken(uint typespec, out byte* ppvSig);
        uint GetNameFromToken(uint tk);
        uint EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rMethods, uint cMax);
        uint GetUserString(uint stk, StringBuilder szString, uint cchString);
        uint GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName);
        uint EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rSignatures, uint cmax);
        uint EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeSpecs, uint cmax);
        uint EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rStrings, uint cmax);
        [PreserveSig]
        int GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam);
        uint EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rCustomAttributes, uint cMax);
        unsafe uint GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob);
        uint FindTypeRef(uint tkResolutionScope, string szName);
        unsafe uint GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue);
        unsafe uint GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr, out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue);
        unsafe uint GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=14)] uint[] rmdOtherMethod, uint cMax);
        unsafe uint GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue);
        unsafe uint GetCustomAttributeByName(uint tkObj, string szName, out void* ppData);
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IsValidToken(uint tk);
        uint GetNestedClassProps(uint tdNestedClass);
        unsafe uint GetNativeCallConvFromSig(void* pvSig, uint cbSig);
        int IsGlobal(uint pd);
    }
}

