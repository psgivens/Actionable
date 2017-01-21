import * as types from '../actions/actionTypes';
import initialState from './initialState';

export default function authenticationReducer(state = initialState.authentication, action) {
  switch (action.type) {
    case types.AUTHENTICATE_REQUEST:
      return {isAuthenticated:false, token:'', error:''};

    case types.AUTHENTICATE_SUCCESS:
      return {isAuthenticated:true, token: action.token, error:''};

    case types.AUTHENTICATE_DENIED:
      return {isAuthenticated:false, token:'', error:action.error};

    case types.LOGOUT_REQUEST:
      return {isAuthenticated:false, token:'', error:''};

    case types.LOGOUT_SUCCESS:
      return {isAuthenticated:false, token:'', error:''};

    case types.AUTHENTICATE_DENIED:
      return {isAuthenticated:false, token:'', error:action.error};

    default:
      return state;
  }
}
