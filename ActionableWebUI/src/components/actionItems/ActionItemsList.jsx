import React, {PropTypes} from 'react';
import ActionItemsListRow from './ActionItemsListRow';

export default class ActionItemsList extends React.Component {
  constructor (props, context) {
    super (props, context);
    this.state = {
      actionItems: []
    };
  }

  render () {
    const {actionItems, deleteActionItem, updateActionItem} = this.props;
    return (
      <div>
        <table className="table">
          <thead>
            <tr>
              <th>Description</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
            </thead>
            <tbody>
              {actionItems.map(actionItem =>
                <ActionItemsListRow
                  key={actionItem.id}
                  actionItem={actionItem}
                  deleteActionItem={deleteActionItem}
                  updateActionItem={updateActionItem}
                  />
              )}
          </tbody>
        </table>
      </div>);
  }
}

ActionItemsList.propTypes = {
  actionItems: PropTypes.array.isRequired,
  deleteActionItem: PropTypes.func.isRequired,
  updateActionItem: PropTypes.func.isRequired
};
