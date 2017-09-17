


// http://stackoverflow.com/questions/30008114/how-do-i-promisify-native-xhr
/*
  Expects the following as opts
  {
    method: String,
    url: String,
    params: String | Object,
    headers: Object
  }
*/

export function makeRequest (opts) {
  return new Promise(function (resolve, reject) {
    const xhr = new XMLHttpRequest();
    xhr.open(opts.method, opts.url);
    xhr.onload = function () {
      if (this.status >= 200 && this.status < 300) {
        resolve(JSON.parse(xhr.response));
      } else {
        reject({
          status: this.status,
          statusText: xhr.statusText && xhr.statusText!=="" ? xhr.statusText : xhr.response
        });
      }
    };
    xhr.onerror = function () {
      reject({
        status: this.status,
        statusText: xhr.statusText && xhr.statusText!=="" ? xhr.statusText : xhr.response
      });
    };
    if (opts.headers) {
      Object.keys(opts.headers).forEach(function (key) {
        xhr.setRequestHeader(key, opts.headers[key]);
      });
    }
    const params = opts.params;

    // TODO: Move JSON.stringify code here

    // // We'll need to stringify if we've been given an object
    // // If we have a string, this is skipped.
    // if (params && typeof params === 'object') {
    //   params = Object.keys(params).map(function (key) {
    //     return encodeURIComponent(key) + '=' + encodeURIComponent(params[key]);
    //   }).join('&');
    // }
    xhr.send(params);
  });
}
