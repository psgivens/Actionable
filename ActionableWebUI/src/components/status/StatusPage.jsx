import React from 'react';
import {connect} from 'react-redux';

function StatusPage (props) {
  const {isAuthenticated, token, authError} = props;
  return (
    <div>
      <h1>Status</h1>
      <table>
        <thead>
          <tr>
            <td>key</td>
            <td>value</td>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>isAuthenticated</td>
            <td>{isAuthenticated.toString()}</td>
          </tr>
          <tr>
            <td>Authentication token</td>
            <td>{token}</td>
          </tr>
          <tr>
            <td>Authentication error</td>
            <td>{authError.toString()}</td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}

function mapStateToProps (state, ownProps) {

  return {
    isAuthenticated: state.authentication.isAuthenticated,
    token: state.authentication.token,
    authError: state.authentication.error
  };
}
function mapDispatchToProps (dispatch) {
  return {};
}

export default connect (mapStateToProps, mapDispatchToProps) (StatusPage);
