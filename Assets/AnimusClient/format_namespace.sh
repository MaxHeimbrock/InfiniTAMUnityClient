#!/bin/bash

nm=$1

for f in *.cs ; 
  do sed -i "11a\namespace ${nm}.${nm}SDK {\n" ${f} && echo '}' >> ${f}
done

sed -i "10a\ \nusing System;" ProtoMessageC.cs
sed -i '/  public ProtoMessageC() : this(animus_robot_sdkPINVOKE.new_ProtoMessageC(), true) {/c \  public unsafe byte[] GetBytes()\n  {\n    global::System.IntPtr cPtr = animus_robot_sdkPINVOKE.ProtoMessageC_data_get(swigCPtr);\n    if (cPtr == global::System.IntPtr.Zero)\n    {\n      return null;\n    }\n    \n    Span<byte> byteArray = new Span<byte>(cPtr.ToPointer(), (int)this.len);\n    return byteArray.ToArray();\n  }\n\n  public ProtoMessageC() : this(animus_robot_sdkPINVOKE.new_ProtoMessageC(), true) {' ProtoMessageC.cs
sed -i '/  public ProtoMessageC() : this(animus_client_sdkPINVOKE.new_ProtoMessageC(), true) {/c \  public unsafe byte[] GetBytes()\n  {\n    global::System.IntPtr cPtr = animus_client_sdkPINVOKE.ProtoMessageC_data_get(swigCPtr);\n    if (cPtr == global::System.IntPtr.Zero)\n    {\n      return null;\n    }\n    \n    Span<byte> byteArray = new Span<byte>(cPtr.ToPointer(), (int)this.len);\n    return byteArray.ToArray();\n  }\n\n  public ProtoMessageC() : this(animus_client_sdkPINVOKE.new_ProtoMessageC(), true) {' ProtoMessageC.cs
