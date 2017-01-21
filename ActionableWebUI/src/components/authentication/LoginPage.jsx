import React from 'react';
import AuthenticationForm from './AuthenticationForm';
import {connect} from 'react-redux';
import {bindActionCreators} from 'redux';
import * as authenticationActions from '../../actions/authenticationActions';

class LoginPage extends React.Component {
  constructor (props) {
    super(props);

    this.state = {credentials:{username:'Bravo@c.com', password1:'Password1!', password2:'Password1!'}};
    this.onAuthenticate = this.onAuthenticate.bind(this);
    this.onChange = this.onChange.bind(this);
    this.updateCredentialsState = this.updateCredentialsState.bind(this);
  }

  onAuthenticate (event) {
    event.preventDefault ();
    this.props.actions.requestAuthentication({
      credentials: this.state.credentials,
      nextPathname: this.props.nextPathname});}

  updateCredentialsState (event) {
    const field = event.target.name;
    let credentials = this.state.credentials;
    credentials[field] = event.target.value;
    return this.setState({credentials: credentials});
  }

  onChange (event) {
    event.preventDefault ();
    this.updateCredentialsState (event);
  }

  render () {
    const errors = {};
    return (
      <div>
        <h1>Login page</h1>
        Is Authenticated: {this.props.isAuthenticated.toString()}<br />
        Next path: {this.props.nextPathname}
        <AuthenticationForm
          credentials={this.state.credentials}
          onAuthenticate={this.onAuthenticate}
          onChange={this.onChange}
          errors={errors}
          />
      </div>
    );
  }
}

function mapStateToProps(state, ownProps) {
  return {
    isAuthenticated: state.authentication.isAuthenticated,
    nextPathname: ownProps.location.state ? ownProps.location.state.nextPathname : "/status"
  };
}

function mapDispatchToProps(dispatch) {
  return {
    actions: bindActionCreators(authenticationActions, dispatch)
  };
}

export default connect(mapStateToProps, mapDispatchToProps) (LoginPage);
