export const getIdpBaseUrl = (ownUrl: string, injectedUrl: string): string => {
  // TODO: Check how this can be overriden from another env
  // users other than mrsauravsahu should not be affected
  if (ownUrl.match("localhost")) 
    return "http://localhost:8084"
  if(ownUrl.match("local."))
    return ownUrl.replace("payobills.", "idp.") 
  return injectedUrl
}

