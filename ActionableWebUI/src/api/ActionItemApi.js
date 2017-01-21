import delay from './delay';
import {makeRequest} from './apiHelpers';

// This file mocks a web API by working with the hard-coded data below.
// It uses setTimeout to simulate the delay of an AJAX call.
// All calls return promises.
const actionItems = [
  {
    id: '1',
    title: 'Create a todo list. Great!',
    description: 'Create a todo list',
    status: '0'
  },
  {
    id: '2',
    title: 'Update your todo list',
    description: 'Update your todo list',
    status: '0'
  },
  {
    id: '3',
    title: 'Complete your todo list',
    description: 'Complete your todo list',
    status: '0'
  }
];


export default class ActionItemApi {
  static getAllActionItems (authentication) {
    return makeRequest({
      method: "GET",
      url: "http://localhost:2360/api/v1/Actions",
      params: "",
      headers: {
        "Authorization": "Bearer " + authentication.token,
        "Content-type": "application/json"
      }
    });
  }

  static saveActionItem (authentication, actionItem) {
    return makeRequest ({
      method: "POST",
      url: "http://localhost:2360/api/v1/Actions",
      params: JSON.stringify(actionItem),
      headers: {
        "Authorization": "Bearer " + authentication.token,
        "Content-type": "application/json"
      }
    });}

  static updateActionItem (authentication, actionItem) {
    return makeRequest ({
      method: "POST",
      url: "http://localhost:2360/api/v1/Actions",
      params: JSON.stringify(actionItem),
      headers: {
        "Authorization": "Bearer " + authentication.token,
        "Content-type": "application/json"
      }
    });}

  static deleteActionItem (authentication, actionItemId) {
    return makeRequest ({
      method: "DELETE",
      url: "http://localhost:2360/api/v1/Actions",
      params: JSON.stringify({actionItemId}),
      headers: {
        "Authorization": "Bearer " + authentication.token,
        "Content-type": "application/json"
      }
    });}
}

//export default ActionItemApi;
