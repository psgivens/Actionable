import delay from './delay';

// This file mocks a web API by working with the hard-coded data below.
// It uses setTimeout to simulate the delay of an AJAX call.
// All calls return promises.
const actionItems = [
  {
    id: '1',
    title: 'Create a todo list',
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

//This would be performed on the server in a real app. Just stubbing in.
const generateId = (actionItem) => {
  return actionItem.id + '-' + actionItem.description.toLowerCase();
};

export default class ActionItemApi {
  static getAllActionItems (authentication) {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        if (authentication.isAuthenticated) {
          resolve(Object.assign([], actionItems));
        } else {
          resolve ([]);
        }
      }, delay);
    });
  }

  static saveActionItem (actionItem) {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
      //   // Simulate server-side validation
      //   const minAuthorNameLength = 3;
      //   if (author.firstName.length < minAuthorNameLength) {
      //     reject(`First Name must be at least ${minAuthorNameLength} characters.`);
      //   }
      //
      //   if (author.lastName.length < minAuthorNameLength) {
      //     reject(`Last Name must be at least ${minAuthorNameLength} characters.`);
      //   }
      //
      //   if (author.id) {
      //     const existingAuthorIndex = authors.findIndex(a => a.id == author.id);
      //     authors.splice(existingAuthorIndex, 1, author);
      //   } else {
      //     //Just simulating creation here.
      //     //The server would generate ids for new authors in a real app.
      //     //Cloning so copy returned is passed by value rather than by reference.
      //     author.id = generateId(author);
      //     authors.push(author);
      //   }

        actionItems.push(actionItem);
        resolve(Object.assign({}, actionItem));
      }, delay);
    });
  }

  static updateActionItem (actionItem) {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        const indexOfActionItem = actionItems.findIndex(item => {
          return item.id == actionItem.id;
        });
        const newItem = Object.assign({}, actionItem);
        actionItems[indexOfActionItem] = newItem;
        resolve(newItem);
      }, delay);
    });
  }

  static deleteActionItem (actionItemId) {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        const indexOfActionItem = actionItems.findIndex(actionItem => {
          actionItem.id == actionItemId;
        });
        actionItems.splice(indexOfActionItem, 1);
        resolve();
      }, delay);
    });
  }
}
