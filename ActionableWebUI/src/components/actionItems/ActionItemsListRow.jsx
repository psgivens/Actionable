import React, {PropTypes} from 'react';
import Button from '../../common/Button';
import {hashHistory} from 'react-router';

export default class ActionItemsListRow extends React.Component {
  constructor (props, context) {
    super (props, context);
    this.onDeleteClick = this.onDeleteClick.bind(this);
    this.onUpdateClick = this.onUpdateClick.bind(this);
    this.onModifyClick = this.onModifyClick.bind(this);
  }

  onDeleteClick (event) {
    event.preventDefault ();
    this.props.deleteActionItem(this.props.actionItem.id);
  }

  onUpdateClick (event) {
    event.preventDefault ();
    this.props.updateActionItem (this.props.actionItem);
  }

  onModifyClick (event) {
    event.preventDefault ();
    console.log ("onModifyClick");
    hashHistory.push("/editactionitem/" + this.props.actionItem.id);
  }

  render () {
    const {actionItem} = this.props;
    return (
      <tr>
        <td>{actionItem.fields["actionable.title"]}</td>
        <td>{actionItem.fields["actionable.status"]}</td>
        <td><Button text="Delete" onClick={this.onDeleteClick}/></td>
        <td><Button text="Update" onClick={this.onUpdateClick}/></td>
        <td><Button text="Modify" onClick={this.onModifyClick}/></td>
      </tr>);
  }
}

ActionItemsListRow.propTypes = {
  actionItem: PropTypes.object.isRequired,
  deleteActionItem: PropTypes.func.isRequired,
  updateActionItem: PropTypes.func.isRequired
};
