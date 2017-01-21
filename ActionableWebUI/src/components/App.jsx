// This component handles the App template used on every page.
import React, {PropTypes} from 'react';
//import Header from './common/Header';
import {connect} from 'react-redux';
import Header from '../core/Header';

class App extends React.Component {

  render() {
    const {children, isAuthenticated} = this.props;
    return (
      <div className="container-fluid">
        <Header isAuthenticated={isAuthenticated}/>
        {children}
      </div>
    );
  }
}

App.propTypes = {
  children: PropTypes.object.isRequired // magic variable created by routes.jsx
  // loading: PropTypes.bool.isRequired
};

function mapStateToProps(state, ownProps) {
  return {
    loading: state.ajaxCallsInProgress > 0,
      isAuthenticated: state.authentication.isAuthenticated
  };
}

export default connect(mapStateToProps)(App);
