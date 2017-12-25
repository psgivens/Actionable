import delay from './delay';
import {makeRequest} from './apiHelpers';


export default class AuthenticationApi {
  static authenticateUser (credentials) {
    let formBody = [];
    for (const property in credentials) {
      const encodedKey = encodeURIComponent(property);
      const encodedValue = encodeURIComponent(credentials[property]);
      formBody.push(encodedKey + "=" + encodedValue);
    }
    formBody = formBody.join("&");

    // return new Promise ((response, error => {}));
    return makeRequest({
      method: "POST",
      url: "http://localhost:2360/Token",
      params: formBody,
      headers: {
        "Content-type": "application/x-www-form-urlencoded; charset=UTF-8"
      },
      success: function(result) { console.log("success: "); console.log(result);},
      error: function(result) { console.log("error");}
    });
  }
}
