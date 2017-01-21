import delay from './delay';

const authentication = {
  isAuthenticated:false,
  token:'',
  error:''
};

export default class AuthenticationApi {
  static authenticateUser (credentials) {
    return new Promise((resolve, reject) => {
      resolve(Object.assign(authentication, {
        isAuthenticated: true,
        token: '1234', error:''
      }));
    }, delay);
  }
}
