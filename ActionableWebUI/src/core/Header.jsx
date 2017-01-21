"use strict";

import React from 'react';
import { Router, Link } from 'react-router';
import NavLink from '../common/NavLink';
import {connect} from 'react-redux';

const Header = ({isAuthenticated}) => {
        const unAuthenticated = <nav className="navbar navbar-default">
          <div className="container-fluid">
              <ul className="nav navbar-nav">
                <li><NavLink to="/login" >Login</NavLink></li>
                <li><NavLink to="/status" >Status</NavLink></li>
              </ul>
          </div>
        </nav>;
        const authenticated = <nav className="navbar navbar-default">
          <div className="container-fluid">
              <ul className="nav navbar-nav">
                <li><NavLink to="/actions" >Actions</NavLink></li>
                <li><NavLink to="/status" >Status</NavLink></li>
                <li><NavLink to="/logout" >Logout</NavLink></li>
              </ul>
          </div>
        </nav>;
        return isAuthenticated ? authenticated : unAuthenticated;
    }

Header.propTypes = {
    isAuthenticated: React.PropTypes.bool.isRequired
}

export default Header;
