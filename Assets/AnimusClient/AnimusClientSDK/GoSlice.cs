//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


namespace AnimusClient.AnimusClientSDK {

public class GoSlice : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal GoSlice(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(GoSlice obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~GoSlice() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          animus_client_sdkPINVOKE.delete_GoSlice(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public SWIGTYPE_p_void data {
    set {
      animus_client_sdkPINVOKE.GoSlice_data_set(swigCPtr, SWIGTYPE_p_void.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = animus_client_sdkPINVOKE.GoSlice_data_get(swigCPtr);
      SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
      return ret;
    } 
  }

  public long len {
    set {
      animus_client_sdkPINVOKE.GoSlice_len_set(swigCPtr, value);
    } 
    get {
      long ret = animus_client_sdkPINVOKE.GoSlice_len_get(swigCPtr);
      return ret;
    } 
  }

  public long cap {
    set {
      animus_client_sdkPINVOKE.GoSlice_cap_set(swigCPtr, value);
    } 
    get {
      long ret = animus_client_sdkPINVOKE.GoSlice_cap_get(swigCPtr);
      return ret;
    } 
  }

  public GoSlice() : this(animus_client_sdkPINVOKE.new_GoSlice(), true) {
  }

}
}
