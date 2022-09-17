namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BA3FEE4C-ECB9-4e41-83B7-183FA41CD859")]
    internal interface IMetaDataEmit
    {
        void SetModuleProps(string szName);
        void Save(string szFile, uint dwSaveFlags);
        unsafe void SaveToStream(void* pIStream, uint dwSaveFlags);
        uint GetSaveSize(uint fSave);
        unsafe uint DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
        unsafe uint DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser);
        void SetHandler([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk);
        unsafe uint DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags);
        void DefineMethodImpl(uint td, uint tkBody, uint tkDecl);
        unsafe uint DefineTypeRefByName(uint tkResolutionScope, char* szName);
        unsafe uint DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit);
        unsafe uint DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob);
        unsafe uint DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent);
        unsafe uint DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
        unsafe void SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize);
        void DeleteClassLayout(uint td);
        unsafe void SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType);
        void DeleteFieldMarshal(uint tk);
        unsafe uint DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
        void SetRVA(uint md, uint ulRVA);
        unsafe uint GetTokenFromSig(byte* pvSig, uint cbSig);
        uint DefineModuleRef(string szName);
        void SetParent(uint mr, uint tk);
        unsafe uint GetTokenFromTypeSpec(byte* pvSig, uint cbSig);
        unsafe void SaveToMemory(void* pbData, uint cbData);
        uint DefineUserString(string szString, uint cchString);
        void DeleteToken(uint tkObj);
        void SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags);
        unsafe void SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements);
        unsafe void SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods);
        unsafe uint SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission);
        void DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
        void SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);
        void DeletePinvokeMap(uint tk);
        unsafe uint DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute);
        unsafe void SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute);
        unsafe uint DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        unsafe uint DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
        unsafe uint DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        unsafe void SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        unsafe void SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods);
        unsafe void SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue);
        uint DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs);
        void ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)] object pImport);
        unsafe uint TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax);
        void SetMethodImplFlags(uint md, uint dwImplFlags);
        void SetFieldRVA(uint fd, uint ulRVA);
        void Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)] object pHandler);
        void MergeEnd();
    }
}

